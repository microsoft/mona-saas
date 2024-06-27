// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mona.SaaS.Core.Constants;
using Mona.SaaS.Core.Enumerations;
using Mona.SaaS.Core.Extensions;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.Web.Extensions;
using Mona.SaaS.Web.Models;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Events = Mona.SaaS.Core.Models.Events;

namespace Mona.SaaS.Web.Controllers
{
    public class SubscriptionController : Controller
    {
        public const string SubscriptionDetailQueryParameter = "_sub";

        private const string objectIdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        private const string tenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";

        public static class ErrorCodes
        {
            public const string UnableToResolveMarketplaceToken = "UnableToResolveMarketplaceToken";
            public const string SubscriptionNotFound = "SubscriptionNotFound";
            public const string SubscriptionActivationFailed = "SubscriptionActivationFailed";
        }

        private readonly DeploymentConfiguration deploymentConfig;

        private readonly ILogger logger;
        private readonly IMarketplaceOperationService mpOperationService;
        private readonly IMarketplaceSubscriptionService mpSubscriptionService;
        private readonly IPublisherConfigurationStore publisherConfigStore;
        private readonly ISubscriptionEventPublisher subscriptionEventPublisher;
        private readonly ISubscriptionStagingCache subscriptionStagingCache;
        private readonly ISubscriptionTestingCache subscriptionTestingCache;

        public SubscriptionController(
            IOptionsSnapshot<DeploymentConfiguration> deploymentConfig,
            ILogger<SubscriptionController> logger,
            IMarketplaceOperationService mpOperationService,
            IMarketplaceSubscriptionService mpSubscriptionService,
            IPublisherConfigurationStore publisherConfigStore,
            ISubscriptionEventPublisher subscriptionEventPublisher,
            ISubscriptionStagingCache subscriptionStagingCache,
            ISubscriptionTestingCache subscriptionTestingCache)
        {
            this.mpOperationService = mpOperationService;
            this.deploymentConfig = deploymentConfig.Value;
            this.logger = logger;
            this.publisherConfigStore = publisherConfigStore;
            this.mpSubscriptionService = mpSubscriptionService;
            this.subscriptionEventPublisher = subscriptionEventPublisher;
            this.subscriptionStagingCache = subscriptionStagingCache;
            this.subscriptionTestingCache = subscriptionTestingCache;
        }

        [Authorize]
        [HttpPost]
        [Route("/", Name = "landing")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> PostLiveLandingPageAsync(LandingPageModel landingPageModel) => PostLandingPageAsync(landingPageModel);

        [Authorize(Policy = "admin")]
        [HttpPost]
        [Route("/test", Name = "landing/test")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> PostTestLandingPageAsync(LandingPageModel landingPageModel)
        {
            if (this.deploymentConfig.IsTestModeEnabled)
            {
                return PostLandingPageAsync(landingPageModel, inTestMode: true);
            }
            else
            {
                return Task.FromResult(NotFound() as IActionResult); // Test mode is disabled...
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("/", Name = "landing")]
        public Task<IActionResult> GetLiveLandingPageAsync(string token = null) => GetLandingPageAsync(token);

        [Authorize(Policy = "admin")]
        [HttpGet]
        [Route("/test", Name = "landing/test")]
        public Task<IActionResult> GetTestLandingPageAsync()
        {
            if (this.deploymentConfig.IsTestModeEnabled)
            {
                return GetLandingPageAsync(inTestMode: true);
            }
            else
            {
                return Task.FromResult(NotFound() as IActionResult); // Test mode is disabled...
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("/webhook", Name = "webhook")]
        public Task<IActionResult> ProcessLiveWebhookNotificationAsync([FromBody] WebhookNotification whNotification) => ProcessWebhookNotificationAsync(whNotification);

        [AllowAnonymous]
        [HttpPost]
        [Route("/webhook/test", Name = "webhook/test")]
        public Task<IActionResult> ProcessTestWebhookNotificationAsync([FromBody] WebhookNotification whNotification)
        {
            if (this.deploymentConfig.IsTestModeEnabled)
            {
                return ProcessWebhookNotificationAsync(whNotification, inTestMode: true);
            }
            else
            {
                return Task.FromResult(NotFound() as IActionResult); // Test mode is disabled...
            }
        }

        private async Task<IActionResult> PostLandingPageAsync(LandingPageModel landingPageModel, bool inTestMode = false)
        {
            var publisherConfig = await this.publisherConfigStore.GetPublisherConfiguration();

            try
            {
                var subscription = await TryGetSubscriptionAsync(landingPageModel.SubscriptionId, inTestMode);

                if (subscription == null)
                {
                    // Well, this is awkward. We've presumably sent the user to the subscription landing page which means that we
                    // were indeed able to resolve the subscription but, for some reason, we don't know about it. Let the user know
                    // and prompt them to return to the AppSource/Marketplace offer URL...

                    this.logger.LogError($"Subscription [{landingPageModel.SubscriptionId}] not found.");

                    return View("Index", new LandingPageModel(inTestMode)
                        .WithCurrentUserInformation(User)
                        .WithPublisherInformation(publisherConfig)
                        .WithErrorCode(ErrorCodes.SubscriptionNotFound));
                }
                else
                {
                    return await CompleteSubscriptionPurchase(subscription, publisherConfig, inTestMode);
                }
            }
            catch (Exception ex)
            {
                // Uh oh. Something broke. Log it and let the user know...

                this.logger.LogError(ex,
                    $"An error occurred while try to complete subscription [{landingPageModel.SubscriptionId}] activation. " +
                    $"See inner exception for details.");

                return View("Index", new LandingPageModel(inTestMode)
                    .WithCurrentUserInformation(User)
                    .WithPublisherInformation(publisherConfig)
                    .WithErrorCode(ErrorCodes.SubscriptionActivationFailed));
            }
        }

        private async Task<IActionResult> CompleteSubscriptionPurchase(Subscription subscription, PublisherConfiguration publisherConfig, bool inTestMode)
        {
            await PublishSubscriptionPurchasedEvent(subscription);

            var redirectUrl = publisherConfig.SubscriptionPurchaseConfirmationUrl
                .WithSubscriptionId(subscription.SubscriptionId);

            await subscriptionStagingCache.PutSubscriptionAsync(subscription);

            this.logger.LogInformation($"Subscription [{subscription.SubscriptionId}] purchase confirmed. Redirecting user to [{redirectUrl}]...");

            return Redirect(redirectUrl);
        }

        private async Task<IActionResult> GetLandingPageAsync(string token = null, bool inTestMode = false)
        {
            var publisherConfig = await this.publisherConfigStore.GetPublisherConfiguration();

            if (publisherConfig == null)
            {
                // Not so fast... you need to complete the setup wizard before you can access the landing page.           
                // TODO: Need to think about what happens if a non-admin user accesses the landing page but Mona has not yet been set up. Just return a 404 I guess? Feels kind of clunky...

                return RedirectToRoute("setup");
            }

            if (string.IsNullOrEmpty(token) && !inTestMode)
            {
                // We don't have a token so we aren't coming from the AppSource/Marketplace. Try to redirect to service marketing page...

                this.logger.LogWarning("Landing page reached but no subscription token was provided. Attempting to redirect to service marketing page...");

                return TryToRedirectToServiceMarketingPageUrl(publisherConfig);
            }
            else
            {
                // We have a token so we're almost certainly coming from the AppSource/Marketplace...

                // If we're running in passthrough mode, we don't need to authenticate the user at this step since we're just going
                // to be forwarding them along to the purchase confirmation page anyway. Spent a lot of time thinking about this one --
                // do we actually need to authenticate the user here for security reasons or do we not? If the user isn't seeing any kind of UI
                // to confirm that they've confirmed their purchase here, then I don't think we need to authenticate them because, really,
                // Mona is acting purely as a Marketplace adapter at this point. All it's doing is channeling events from the Marketplace to the
                // SaaS app so Mona's identity -- Identity:AppIdentity -- is plenty to secure the exchange of information and the
                // handing off of the customer to the SaaS app. Really, authing the user here is an uncessary step that just makes
                // the UX worse for the purchaser.

                if (User.Identity.IsAuthenticated || deploymentConfig.IsPassthroughModeEnabled)
                {
                    // The default landing page experience...

                    var subscription = await TryResolveSubscriptionTokenAsync(token, inTestMode);

                    if (subscription == null)
                    {
                        // The Marketplace can't resolve the provided token so we need to kick back an error
                        // to the user and point them back to the original AppSource/Marketplace listing...

                        this.logger.LogWarning($"Unable to resolve source subscription token [{token}].");

                        if (deploymentConfig.IsPassthroughModeEnabled)
                        {
                            // Probably need to think a little more about how we handle this situation.
                            // Right now, we're just kicking back a 404 since we couldn't find the requested
                            // subscription. Do we redirect the user to the purchase confirmation page without 
                            // a _sub parameter and let the SaaS app sort it out? Do we redirect with a status code?
                            // If we redirect with a status code, then there's the possibilty the a bad actor could
                            // spoof the call to the SaaS app and do something nefarious. For now, we'll just kick
                            // back a 404 and talk to some ISV partners to see what they'd prefer to do here.

                            return NotFound($"Unable to resolve source subscription token [{token}].");
                        }
                        else
                        {
                            return View("Index", new LandingPageModel(inTestMode)
                                .WithCurrentUserInformation(User)
                                .WithPublisherInformation(publisherConfig)
                                .WithErrorCode(ErrorCodes.UnableToResolveMarketplaceToken));
                        }
                    }
                    else
                    {
                        // TODO: Just a thought... at this point, we can assume that the user has purchased a subscription through the AppSource/Marketplace but
                        // what if they don't confirm and this is the farthest that they ever get? If that's the case, I'd think that the publisher would want to
                        // be aware of this "almost purchase." Does it make sense to fire a [SubscriptionPurchasing] event here so that this scenario can be tracked
                        // and possibly followed up on by the publisher's sales team? Did something happen that made the user reconsider their purchase?

                        if (subscription.Status == SubscriptionStatus.PendingActivation)
                        {
                            // Score! New customer!

                            if (inTestMode)
                            {
                                await this.subscriptionTestingCache.PutSubscriptionAsync(subscription).ConfigureAwait(false);
                            }

                            if (deploymentConfig.IsPassthroughModeEnabled)
                            {
                                // In passthrough mode, customers never see any Mona UI. Get the subscription details and pass
                                // them directly through to the purchase confirmation page through the _sub parameter.

                                this.logger.LogInformation(
                                    $"Subscription [{subscription.SubscriptionId}] is unknown to Mona. " +
                                    $"Passthrough mode is [enabled]; redirecting user to SaaS purchase confirmation page...");

                                return await CompleteSubscriptionPurchase(subscription, publisherConfig, inTestMode);
                            }
                            else
                            { 
                                // Let's get them over to the landing page so they can complete their purchase and
                                // we can get their subscription spun up...

                                this.logger.LogInformation(
                                    $"Subscription [{subscription.SubscriptionId}] is unknown to Mona. " +
                                    $"Passthrough mode is [disabled]; redirecting user to Mona's landing page...");

                                return View("Index", new LandingPageModel(inTestMode)
                                    .WithCurrentUserInformation(User)
                                    .WithPublisherInformation(publisherConfig)
                                    .WithSubscriptionInformation(subscription));
                            }    
                        }
                        else
                        {
                            // We already know about this subscription. Redirecting to publisher-defined subscription configuration UI...

                            var redirectUrl = publisherConfig.SubscriptionConfigurationUrl.WithSubscriptionId(subscription.SubscriptionId);

                            await this.subscriptionStagingCache.PutSubscriptionAsync(subscription);

                            this.logger.LogInformation(
                                $"Subscription [{subscription.SubscriptionId}] is known to Mona. " +
                                $"Redirecting user to subscription configuration page at [{redirectUrl}]...");

                            return Redirect(redirectUrl);
                        }
                    }
                }
                else
                {
                    // User needs to authenticate first...

                    this.logger.LogWarning($"User has provided a subscription token [{token}] but has not yet been authenticated. Challenging...");

                    return Challenge();
                }
            }
        }

        private string AppendSubscriptionAccessTokenToUrl(string url, string subToken) =>
            $"{url}{(string.IsNullOrEmpty(new Uri(url).Query) ? "?" : "&")}{SubscriptionDetailQueryParameter}={WebUtility.UrlEncode(subToken)}";

        private async Task<IActionResult> ProcessWebhookNotificationAsync(WebhookNotification whNotification, bool inTestMode = false)
        {
            try
            {
                var subscription = await TryGetSubscriptionAsync(whNotification.SubscriptionId, inTestMode);

                if (subscription == null)
                {
                    // We don't even know about this subscription...

                    this.logger.LogError(
                        $"Unable to process Marketplace webhook notification [{whNotification.OperationId}]. " +
                        $"Subscription [{whNotification.SubscriptionId}] not found.");

                    return NotFound();
                }
                else
                {
                    // Let's look at the action type and decide how to handle it...

                    var opType = ToCoreOperationType(whNotification.ActionType);

                    this.logger.LogInformation($"Processing subscription [{subscription.SubscriptionId}] webhook [{opType}] operation [{whNotification.OperationId}]...");

                    // Make sure that the Marketplace actually sent this notification...

                    await VerifyWebhookNotificationAsync(whNotification, inTestMode);

                    // Depending on the type of action, publish an appropriate event...

                    await PublishWebhookSubscriptionEvent(opType, subscription, whNotification);

                    // If we're in test mode, cache the subscription model for later.

                    if (inTestMode)
                    {
                        await subscriptionTestingCache.PutSubscriptionAsync(subscription);
                    }
                    else
                    {
                        // Otherwise, notify the Marketplace that the webhook notification has been successfully processed.

                        await mpOperationService.ConfirmOperationComplete(whNotification.SubscriptionId, whNotification.OperationId);
                    }

                    this.logger.LogInformation($"Subscription [{subscription.SubscriptionId}] webhook [{opType}] operation [{whNotification.OperationId}] processed successfully.");

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                // Uh oh... something else broke. Log it and let the Marketplace know. If it's important, hopefully they'll call us back...

                this.logger.LogError(ex,
                    $"An error occurred while trying to process Marketplace webhook notification [{whNotification.OperationId}]. " +
                    $"See inner exception for details.");

                return StatusCode((int)(HttpStatusCode.InternalServerError));
            }
        }

        private Task PublishSubscriptionPurchasedEvent(Subscription subscription) =>
            this.deploymentConfig.EventVersion switch
            {
                EventVersions.V_2021_05_01 =>
                this.subscriptionEventPublisher.PublishEventAsync(new Events.V_2021_05_01.SubscriptionPurchased(subscription)),

                EventVersions.V_2021_10_01 =>
                this.subscriptionEventPublisher.PublishEventAsync(new Events.V_2021_10_01.SubscriptionPurchased(subscription)),

                _ => throw new NotSupportedException($"Subscription event version [{this.deploymentConfig.EventVersion}] not supported.")
            };

        private Task PublishWebhookSubscriptionEvent(SubscriptionOperationType opType, Subscription subscription, WebhookNotification whNotification) =>
            this.deploymentConfig.EventVersion switch
            {
                EventVersions.V_2021_05_01 =>
                PublishSubscriptionEvent_V_2021_05_01(opType, subscription, whNotification),

                EventVersions.V_2021_10_01 =>
                PublishSubscriptionEvent_V_2021_10_01(opType, subscription, whNotification),

                _ => throw new NotSupportedException($"Subscription event version [{this.deploymentConfig.EventVersion}] not supported.")
            };

        private Task PublishSubscriptionEvent_V_2021_05_01(SubscriptionOperationType opType, Subscription subscription, WebhookNotification whNotification) =>
            opType switch
            {
                SubscriptionOperationType.Cancel =>
                this.subscriptionEventPublisher.PublishEventAsync(
                    new Events.V_2021_05_01.SubscriptionCanceled(subscription, whNotification.OperationId)),

                SubscriptionOperationType.ChangePlan =>
                this.subscriptionEventPublisher.PublishEventAsync(
                    new Events.V_2021_05_01.SubscriptionPlanChanged(subscription, whNotification.OperationId, whNotification.PlanId)),

                SubscriptionOperationType.ChangeSeatQuantity =>
                this.subscriptionEventPublisher.PublishEventAsync(
                    new Events.V_2021_05_01.SubscriptionSeatQuantityChanged(subscription, whNotification.OperationId, whNotification.SeatQuantity)),

                SubscriptionOperationType.Reinstate =>
                this.subscriptionEventPublisher.PublishEventAsync(
                    new Events.V_2021_05_01.SubscriptionReinstated(subscription, whNotification.OperationId)),

                SubscriptionOperationType.Suspend =>
                this.subscriptionEventPublisher.PublishEventAsync(
                    new Events.V_2021_05_01.SubscriptionSuspended(subscription, whNotification.OperationId)),

                SubscriptionOperationType.Renew =>
                this.subscriptionEventPublisher.PublishEventAsync(
                    new Events.V_2021_05_01.SubscriptionRenewed(subscription, whNotification.OperationId)),

                _ => throw new NotSupportedException($"Subscription operation type [{opType}] is unknown.")
            };

        private Task PublishSubscriptionEvent_V_2021_10_01(SubscriptionOperationType opType, Subscription subscription, WebhookNotification whNotification) =>
           opType switch
           {
               SubscriptionOperationType.Cancel =>
               this.subscriptionEventPublisher.PublishEventAsync(
                   new Events.V_2021_10_01.SubscriptionCanceled(subscription, whNotification.OperationId)),

               SubscriptionOperationType.ChangePlan =>
               this.subscriptionEventPublisher.PublishEventAsync(
                   new Events.V_2021_10_01.SubscriptionPlanChanged(subscription, whNotification.OperationId, whNotification.PlanId)),

               SubscriptionOperationType.ChangeSeatQuantity =>
               this.subscriptionEventPublisher.PublishEventAsync(
                   new Events.V_2021_10_01.SubscriptionSeatQuantityChanged(subscription, whNotification.OperationId, whNotification.SeatQuantity)),

               SubscriptionOperationType.Reinstate =>
               this.subscriptionEventPublisher.PublishEventAsync(
                   new Events.V_2021_10_01.SubscriptionReinstated(subscription, whNotification.OperationId)),

               SubscriptionOperationType.Suspend =>
               this.subscriptionEventPublisher.PublishEventAsync(
                   new Events.V_2021_10_01.SubscriptionSuspended(subscription, whNotification.OperationId)),

               SubscriptionOperationType.Renew =>
               this.subscriptionEventPublisher.PublishEventAsync(
                   new Events.V_2021_10_01.SubscriptionRenewed(subscription, whNotification.OperationId)),

               _ => throw new NotSupportedException($"Subscription operation type [{opType}] is unknown.")
           };

        private IActionResult TryToRedirectToServiceMarketingPageUrl(PublisherConfiguration publisherConfig) =>
            string.IsNullOrEmpty(publisherConfig.PublisherHomePageUrl) ? NotFound() as IActionResult : Redirect(publisherConfig.PublisherHomePageUrl);

        private async Task<Subscription> TryGetSubscriptionAsync(string subscriptionId, bool inTestMode = false)
        {
            try
            {
                if (inTestMode)
                {
                    // We're in test mode.
                    // Try to pull the "mock" subscription from local (blob storage by default) cache...

                    this.logger.LogWarning($"[TEST MODE]: Trying to get test subscription [{subscriptionId}] from subscription cache...");

                    return await this.subscriptionTestingCache.GetSubscriptionAsync(subscriptionId);
                }
                else
                {
                    // Try to get the "record of truth" subscription information from the Marketplace...

                    return await this.mpSubscriptionService.GetSubscriptionAsync(subscriptionId);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An error occurred while trying to get subscription [{subscriptionId}]. See inner exception for details.");

                throw;
            }
        }

        private async Task<Subscription> TryResolveSubscriptionTokenAsync(string subscriptionToken, bool inTestMode = false)
        {
            try
            {
                if (inTestMode)
                {
                    // We're in test mode so there's no actual subscription to resolve.
                    // Instead, we'll create a "mock" subscription.

                    return CreateTestSubscription();
                }
                else
                {
                    // Try to get the subscription information from the Marketplace...

                    return await this.mpSubscriptionService.ResolveSubscriptionTokenAsync(subscriptionToken);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An error occurred while attempting to resolve subscription token [{subscriptionToken}]. See exception for details.");

                return null;
            }
        }

        private async Task VerifyWebhookNotificationAsync(WebhookNotification whNotification, bool inTestMode = false)
        {
            if (inTestMode)
            {
                this.logger.LogWarning(
                    $"[TEST MODE]: Verifying subscription [{whNotification.SubscriptionId}] operation " +
                    $"[{whNotification.OperationId}] in test mode. Bypassing Marketplace API and continuing...");
            }
            else
            {
                var operation = await this.mpOperationService.GetSubscriptionOperationAsync(
                    whNotification.SubscriptionId, whNotification.OperationId);

                if (operation == null ||
                    operation.OperationId != whNotification.OperationId ||
                    operation.OperationType != ToCoreOperationType(whNotification.ActionType) ||
                    operation.SubscriptionId != whNotification.SubscriptionId)
                {
                    throw new ApplicationException($"Unable to verify subscription [{whNotification.SubscriptionId}] operation [{whNotification.OperationId}].");
                }
            }
        }

        private SubscriptionOperationType ToCoreOperationType(string mpActionType) =>
            mpActionType switch
            {
                MarketplaceActionTypes.ChangePlan =>
                SubscriptionOperationType.ChangePlan,

                MarketplaceActionTypes.ChangeQuantity =>
                SubscriptionOperationType.ChangeSeatQuantity,

                MarketplaceActionTypes.Reinstate =>
                SubscriptionOperationType.Reinstate,

                MarketplaceActionTypes.Suspend =>
                SubscriptionOperationType.Suspend,

                MarketplaceActionTypes.Unsubscribe =>
                SubscriptionOperationType.Cancel,

                MarketplaceActionTypes.Renew =>
                SubscriptionOperationType.Renew,

                _ => throw new ArgumentException($"Action type [{mpActionType}] unknown.")
            };

        private Subscription CreateTestSubscription() => new Subscription
        {
            SubscriptionId = TryGetQueryStringParameter(TestSubscriptionParameterNames.SubscriptionId, Guid.NewGuid().ToString()),
            SubscriptionName = TryGetQueryStringParameter(TestSubscriptionParameterNames.SubscriptionName, "Test Subscription"),
            OfferId = TryGetQueryStringParameter(TestSubscriptionParameterNames.OfferId, "Test Offer"),
            PlanId = TryGetQueryStringParameter(TestSubscriptionParameterNames.PlanId, "Test Plan"),
            IsTest = true,
            IsFreeTrial = TryParseBooleanQueryStringParameter(TestSubscriptionParameterNames.IsFreeTrial, false).Value,
            SeatQuantity = TryParseIntQueryStringParameter(TestSubscriptionParameterNames.SeatQuantity),
            Term = CreateTestMarketplaceTerm(),
            Beneficiary = CreateTestMarketplaceBeneficiary("beneficiary@microsoft.com"),
            Purchaser = CreateTestMarketplacePurchaser("purchaser@microsoft.com"),
            Status = SubscriptionStatus.PendingActivation
        };

        private MarketplaceTerm CreateTestMarketplaceTerm() => new MarketplaceTerm
        {
            EndDate = TryParseDateTimeQueryStringParameter(TestSubscriptionParameterNames.TermEndDate, DateTime.UtcNow.Date.AddMonths(1)),
            StartDate = TryParseDateTimeQueryStringParameter(TestSubscriptionParameterNames.TermStartDate, DateTime.UtcNow.Date),
            TermUnit = TryGetQueryStringParameter(TestSubscriptionParameterNames.TermUnit, "PT1M")
        };

        private MarketplaceUser CreateTestMarketplaceBeneficiary(string defaultUserEmail) => new MarketplaceUser
        {
            AadObjectId = TryGetQueryStringParameter(TestSubscriptionParameterNames.BeneficiaryAadObjectId, User.FindFirstValue(objectIdClaimType)),
            AadTenantId = TryGetQueryStringParameter(TestSubscriptionParameterNames.BeneficiaryAadTenantId, User.FindFirstValue(tenantIdClaimType)),
            UserEmail = TryGetQueryStringParameter(TestSubscriptionParameterNames.BeneficiaryUserEmail, defaultUserEmail),
            UserId = TryGetQueryStringParameter(TestSubscriptionParameterNames.BeneficiaryUserId, User.FindFirstValue(ClaimTypes.Upn))
        };

        private MarketplaceUser CreateTestMarketplacePurchaser(string defaultUserEmail) => new MarketplaceUser
        {
            AadObjectId = TryGetQueryStringParameter(TestSubscriptionParameterNames.PurchaserAadObjectId, User.FindFirstValue(objectIdClaimType)),
            AadTenantId = TryGetQueryStringParameter(TestSubscriptionParameterNames.PurchaserAadTenantId, User.FindFirstValue(tenantIdClaimType)),
            UserEmail = TryGetQueryStringParameter(TestSubscriptionParameterNames.PurchaserUserEmail, defaultUserEmail),
            UserId = TryGetQueryStringParameter(TestSubscriptionParameterNames.PurchaserUserId, User.FindFirstValue(ClaimTypes.Upn))
        };

        private string TryGetQueryStringParameter(string key, string defaultValue = null) =>
            (Request.Query.TryGetValue(key, out var value) ? value.ToString() : defaultValue);

        private bool? TryParseBooleanQueryStringParameter(string key, bool? defaultValue = null) =>
            (Request.Query.TryGetValue(key, out var value) ? bool.Parse(value.ToString()) : defaultValue);

        private DateTime? TryParseDateTimeQueryStringParameter(string key, DateTime? defaultValue = null) =>
            (Request.Query.TryGetValue(key, out var value) ? DateTime.Parse(value.ToString()) : defaultValue);

        private int? TryParseIntQueryStringParameter(string key, int? defaultValue = null) =>
            (Request.Query.TryGetValue(key, out var value) ? int.Parse(value.ToString()) : defaultValue);

        public static class TestSubscriptionParameterNames
        {
            public const string SubscriptionId = "subscriptionId";
            public const string SubscriptionName = "subscriptionName";
            public const string OfferId = "offerId";
            public const string PlanId = "planId";
            public const string IsFreeTrial = "isFreeTrial";
            public const string SeatQuantity = "seatQuantity";
            public const string TermStartDate = "term_startDate";
            public const string TermEndDate = "term_endDate";
            public const string TermUnit = "term_termUnit";
            public const string BeneficiaryAadObjectId = "beneficiary_aadObjectId";
            public const string BeneficiaryAadTenantId = "beneficiary_aadTenantId";
            public const string BeneficiaryUserEmail = "beneficiary_userEmail";
            public const string BeneficiaryUserId = "beneficiary_userId";
            public const string PurchaserAadObjectId = "purchaser_aadObjectId";
            public const string PurchaserAadTenantId = "purchaser_aadTenantId";
            public const string PurchaserUserEmail = "purchaser_userEmail";
            public const string PurchaserUserId = "purchaser_userId";
        }
    }
}

