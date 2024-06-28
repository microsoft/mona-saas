// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mona.SaaS.Core.Enumerations;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.Core.Models.Web;
using System;
using System.Threading.Tasks;

namespace Mona.SaaS.Services.Web
{
    public class LiveSubscriptionWebService : BaseSubscriptionWebService
    {
        private readonly IMarketplaceOperationService mpOperationService;
        private readonly IMarketplaceSubscriptionService mpSubscriptionService;
        private readonly ISubscriptionStagingCache subscriptionStagingCache;

        public LiveSubscriptionWebService(
            IOptionsSnapshot<DeploymentConfiguration> deploymentConfigSnap,
            ILogger<LiveSubscriptionWebService> log,
            IPublisherConfigurationStore publisherConfigStore,
            ISubscriptionEventPublisher subscriptionEventPublisher,
            IMarketplaceOperationService mpOperationService,
            IMarketplaceSubscriptionService mpSubscriptionService,
            ISubscriptionStagingCache subscriptionStagingCache)
            : base(deploymentConfigSnap, log, publisherConfigStore, subscriptionEventPublisher)
        {
            this.mpOperationService = mpOperationService;
            this.mpSubscriptionService = mpSubscriptionService;
            this.subscriptionStagingCache = subscriptionStagingCache;
        }

        public override async Task<IActionResult> OnLanding(HttpContext httpContext, string subToken = null)
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            try
            {
                var publisherConfig = await GetPublisherConfiguration();

                if (publisherConfig == null)
                {
                    // Publisher configuration not found.
                    // We have no idea who this is.
                    // Play it safe and return a 404.

                    log.LogWarning(
                        "Live landing page reached but Mona hasn't been set up yet. " +
                        "Returning 404: Not Found...");

                    return new NotFoundResult();
                }

                if (string.IsNullOrEmpty(subToken))
                {
                    // No token so we definitely aren't coming from the Marketplace.
                    // Bounce them to the marketing page.

                    log.LogWarning(
                        "Live landing page reached but no subscription token was provided. " +
                        "Attempting to redirect to marketing page...");

                    return await TryRedirectToMarketingPage();
                }

                // We have a token so we're almost certainly coming from the Marketplace.
                // We validate the token by using it to try to get subscription details from the Marketplace.

                var subscription = await mpSubscriptionService.ResolveSubscriptionTokenAsync(subToken);

                if (subscription == null)
                {
                    // Whoa there... something's not right. We have a token but we can't redeem it for subscription information.
                    // Assume breach. Zero trust. It's time to bounce them out of here.

                    log.LogWarning(
                        "Live landing page reached with subscription token but the Marketplace doesn't recognize it. " +
                        "Sus. Returning 404: Not Found...");

                    return new NotFoundResult();
                }

                if (subscription.Status == SubscriptionStatus.PendingActivation)
                {
                    // Score! New customer!

                    return await CompleteSubscriptionPurchaseJourney(subscription);
                }
                else
                {
                    // We already know about this subscription.
                    // Redirecting to publisher-defined subscription configuration UI...

                    return await CompleteSubscriptionConfigurationJourney(subscription);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex,
                    "An error occurred while handling a live subscription purchase landing. " +
                    "See exception for more details.");

                throw;
            }
        }

        public override async Task<IActionResult> OnWebhookNotification(HttpContext httpContext, WebhookNotification whNotification)
        {
            try
            {
                var subscription = await mpSubscriptionService.GetSubscriptionAsync(whNotification.SubscriptionId);

                if (subscription == null)
                {
                    // We don't even know about this subscription...

                    log.LogWarning(
                        $"Unable to process Marketplace webhook notification [{whNotification.OperationId}]. " +
                        $"Subscription [{whNotification.SubscriptionId}] not found.");

                    return new NotFoundResult();
                }
                else
                {
                    var opType = ToCoreOperationType(whNotification.ActionType);

                    log.LogInformation(
                        $"Processing subscription [{subscription.SubscriptionId}] webhook [{opType}] " +
                        $"operation [{whNotification.OperationId}]...");

                    await VerifyWebhookNotificationAsync(whNotification);
                    await PublishWebhookSubscriptionEvent(opType, subscription, whNotification);
                    await mpOperationService.ConfirmOperationComplete(whNotification.SubscriptionId, whNotification.OperationId);

                    log.LogInformation(
                        $"Subscription [{subscription.SubscriptionId}] webhook [{opType}] " +
                        $"operation [{whNotification.OperationId}] processed successfully.");

                    return new OkResult();
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex,
                    "An error occurred while handling a live subscription webhook notification. " +
                    "See exception for more details.");

                throw;
            }
        }

        protected override async Task<IActionResult> CompleteSubscriptionConfigurationJourney(Subscription subscription)
        {
            await subscriptionStagingCache.PutSubscriptionAsync(subscription);

            return await base.CompleteSubscriptionConfigurationJourney(subscription);
        }

        protected override async Task<IActionResult> CompleteSubscriptionPurchaseJourney(Subscription subscription)
        {
            await subscriptionStagingCache.PutSubscriptionAsync(subscription);

            return await base.CompleteSubscriptionPurchaseJourney(subscription);
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
                throw new ApplicationException(
                    $"Unable to verify subscription [{whNotification.SubscriptionId}] operation [{whNotification.OperationId}].");
            }
        }
    }
}