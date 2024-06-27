// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mona.SaaS.Core.Constants;
using Mona.SaaS.Core.Enumerations;
using Mona.SaaS.Core.Extensions;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.Web.Models;
using System.Net;
using Events = Mona.SaaS.Core.Models.Events;

namespace Mona.SaaS.Web.Controllers
{
    public class SubscriptionController : Controller
    {
        public static class ErrorCodes
        {
            public const string UnableToResolveMarketplaceToken = "UnableToResolveMarketplaceToken";
            public const string SubscriptionNotFound = "SubscriptionNotFound";
            public const string SubscriptionActivationFailed = "SubscriptionActivationFailed";
        }

        private readonly DeploymentConfiguration deploymentConfig;

        private readonly ILogger log;
        private readonly IMarketplaceOperationService mpOperationService;
        private readonly IMarketplaceSubscriptionService mpSubscriptionService;
        private readonly IPublisherConfigurationStore publisherConfigStore;
        private readonly ISubscriptionEventPublisher subscriptionEventPublisher;
        private readonly ISubscriptionStagingCache subscriptionStagingCache;

        public SubscriptionController(
            IOptionsSnapshot<DeploymentConfiguration> deploymentConfig,
            ILogger<SubscriptionController> log,
            IMarketplaceOperationService mpOperationService,
            IMarketplaceSubscriptionService mpSubscriptionService,
            IPublisherConfigurationStore publisherConfigStore,
            ISubscriptionEventPublisher subscriptionEventPublisher,
            ISubscriptionStagingCache subscriptionStagingCache)
        {
            this.log = log;
            this.mpOperationService = mpOperationService;
            this.deploymentConfig = deploymentConfig.Value;
            this.publisherConfigStore = publisherConfigStore;
            this.mpSubscriptionService = mpSubscriptionService;
            this.subscriptionEventPublisher = subscriptionEventPublisher;
            this.subscriptionStagingCache = subscriptionStagingCache;
        }

        private async Task<IActionResult> CompleteSubscriptionPurchase(Subscription subscription, PublisherConfiguration publisherConfig)
        {
            var redirectUrl = publisherConfig.SubscriptionPurchaseConfirmationUrl
                .WithSubscriptionId(subscription.SubscriptionId);

            await PublishSubscriptionPurchasedEvent(subscription);
            await subscriptionStagingCache.PutSubscriptionAsync(subscription);

            log.LogInformation($"Subscription [{subscription.SubscriptionId}] purchase confirmed. Redirecting user to [{redirectUrl}]...");

            return Redirect(redirectUrl);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("/", Name = "landing")]
        public async Task<IActionResult> OnLanding(string? token = null)
        {
            try
            {
                var publisherConfig = await publisherConfigStore.GetPublisherConfiguration();

                if (publisherConfig == null)
                {
                    // Publisher configuration not found.
                    // We have no idea who this is.
                    // Play it safe and return a 404.

                    log.LogWarning("Landing page reached but Mona hasn't been set up yet. Returning 404 Not Found...");

                    return NotFound();
                }

                if (string.IsNullOrEmpty(token))
                {
                    // No token so we definitely aren't coming from the Marketplace.
                    // Bounce them to the marketing page.

                    log.LogWarning("Landing page reached but no subscription token was provided. Attempting to redirect to service marketing page...");

                    return TryToRedirectToServiceMarketingPageUrl(publisherConfig);
                }

                // We have a token so we're almost certainly coming from the Marketplace.
                // We validate the token by using it to try to get subscription details from the Marketplace.

                var subscription = await TryResolveSubscriptionTokenAsync(token);

                if (subscription == null)
                {
                    // Whoa there... something's not right. We have a token but we can't redeem it for subscription information.
                    // Assume breach. Zero trust. It's time to bounce them out of here.

                    log.LogWarning("Landing page reached and subscription token provided by the Marketplace doesn't recognize it. Returning 404 Not Found...");

                    return NotFound();
                }

                if (subscription.Status == SubscriptionStatus.PendingActivation)
                {
                    // Score! New customer!

                    log.LogInformation(
                        $"New customer! Subscription [{subscription.SubscriptionId}] is unknown to Mona. " +
                         "Redirecting user to SaaS purchase configuration page.");

                    return await CompleteSubscriptionPurchase(subscription, publisherConfig);
                }
                else
                {
                    // We already know about this subscription. Redirecting to publisher-defined subscription configuration UI...

                    var redirectUrl = publisherConfig.SubscriptionConfigurationUrl.WithSubscriptionId(subscription.SubscriptionId);

                    await subscriptionStagingCache.PutSubscriptionAsync(subscription);

                    this.log.LogInformation(
                        $"Subscription [{subscription.SubscriptionId}] is known to Mona. " +
                        $"Redirecting user to subscription configuration page at [{redirectUrl}]...");

                    return Redirect(redirectUrl);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred on landing. See inner exception for more details.");

                return StatusCode((int)(HttpStatusCode.InternalServerError));
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("/webhook", Name = "webhook")]
        private async Task<IActionResult> OnWehbookNotification([FromBody] WebhookNotification whNotification)
        {
            try
            {
                var subscription = await TryGetSubscriptionAsync(whNotification.SubscriptionId);

                if (subscription == null)
                {
                    // We don't even know about this subscription...

                    this.log.LogError(
                        $"Unable to process Marketplace webhook notification [{whNotification.OperationId}]. " +
                        $"Subscription [{whNotification.SubscriptionId}] not found.");

                    return NotFound();
                }
                else
                {
                    var opType = ToCoreOperationType(whNotification.ActionType);

                    log.LogInformation($"Processing subscription [{subscription.SubscriptionId}] webhook [{opType}] operation [{whNotification.OperationId}]...");

                    await VerifyWebhookNotificationAsync(whNotification);
                    await PublishWebhookSubscriptionEvent(opType, subscription, whNotification);
                    await mpOperationService.ConfirmOperationComplete(whNotification.SubscriptionId, whNotification.OperationId);

                    log.LogInformation($"Subscription [{subscription.SubscriptionId}] webhook [{opType}] operation [{whNotification.OperationId}] processed successfully.");

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                // Uh oh... something else broke. Log it and let the Marketplace know. If it's important, hopefully they'll call us back...

                log.LogError(ex,
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

        private async Task<Subscription> TryGetSubscriptionAsync(string subscriptionId)
        {
            try
            {
                // Try to get the "record of truth" subscription information from the Marketplace...

                return await mpSubscriptionService.GetSubscriptionAsync(subscriptionId);
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"An error occurred while trying to get subscription [{subscriptionId}]. See inner exception for details.");

                throw;
            }
        }

        private async Task<Subscription> TryResolveSubscriptionTokenAsync(string subscriptionToken)
        {
            try
            {
                // Try to get the subscription information from the Marketplace...

                return await mpSubscriptionService.ResolveSubscriptionTokenAsync(subscriptionToken);
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"An error occurred while attempting to resolve subscription token [{subscriptionToken}]. See exception for details.");

                return null;
            }
        }

        private async Task VerifyWebhookNotificationAsync(WebhookNotification whNotification)
        {
            var operation = await mpOperationService.GetSubscriptionOperationAsync(
                whNotification.SubscriptionId, whNotification.OperationId);

            if (operation == null ||
                operation.OperationId != whNotification.OperationId ||
                operation.OperationType != ToCoreOperationType(whNotification.ActionType) ||
                operation.SubscriptionId != whNotification.SubscriptionId)
            {
                throw new ApplicationException($"Unable to verify subscription [{whNotification.SubscriptionId}] operation [{whNotification.OperationId}].");
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
    }
}

