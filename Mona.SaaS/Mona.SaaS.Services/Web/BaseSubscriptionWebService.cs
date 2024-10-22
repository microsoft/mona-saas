// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mona.SaaS.Core.Constants;
using Mona.SaaS.Core.Enumerations;
using Mona.SaaS.Core.Extensions;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.Core.Models.Web;
using System;
using System.Threading.Tasks;
using Events = Mona.SaaS.Core.Models.Events;

namespace Mona.SaaS.Services.Web
{
    public abstract class BaseSubscriptionWebService : ISubscriptionWebService
    {
        protected readonly DeploymentConfiguration deploymentConfig;
        protected readonly ILogger log;
        protected readonly IPublisherConfigurationStore publisherConfigStore;
        protected readonly ISubscriptionEventPublisher subscriptionEventPublisher;

        protected BaseSubscriptionWebService(
            IOptionsSnapshot<DeploymentConfiguration> deploymentConfigSnap,
            ILogger log,
            IPublisherConfigurationStore publisherConfigStore,
            ISubscriptionEventPublisher subscriptionEventPublisher)
        {
            deploymentConfig = deploymentConfigSnap.Value;
            this.log = log;
            this.publisherConfigStore = publisherConfigStore;
            this.subscriptionEventPublisher = subscriptionEventPublisher;
        }

        public abstract Task<IActionResult> OnLanding(HttpContext httpContext, string subToken = null);
        public abstract Task<IActionResult> OnWebhookNotification(HttpContext httpContext, WebhookNotification whNotification);

        private PublisherConfiguration _publisherConfig = null;

        protected async Task<PublisherConfiguration> GetPublisherConfiguration() =>
            _publisherConfig ??= await publisherConfigStore.GetPublisherConfiguration();

        protected virtual async Task<IActionResult> CompleteSubscriptionConfigurationJourney(Subscription subscription)
        {
            var publisherConfig = await GetPublisherConfiguration();

            var redirectUrl = publisherConfig.SubscriptionConfigurationUrl
                .WithSubscriptionId(subscription.SubscriptionId);

            log.LogInformation(
                $"Subscription [{subscription.SubscriptionId}] is known to Mona. " +
                $"Redirecting user to subscription configuration page at [{redirectUrl}]...");

            return new RedirectResult(redirectUrl);
        }

        protected virtual async Task<IActionResult> CompleteSubscriptionPurchaseJourney(Subscription subscription)
        {
            var publisherConfig = await GetPublisherConfiguration();

            var redirectUrl = publisherConfig.SubscriptionPurchaseConfirmationUrl
                .WithSubscriptionId(subscription.SubscriptionId);

            await PublishSubscriptionPurchasedEvent(subscription);

            log.LogInformation(
                $"Subscription [{subscription.SubscriptionId}] purchase confirmed. " + 
                $"Redirecting user to [{redirectUrl}]...");

            return new RedirectResult(redirectUrl);
        }

        protected async Task<IActionResult> TryRedirectToMarketingPage()
        {
            var publisherConfig = await GetPublisherConfiguration();

            if (string.IsNullOrEmpty(publisherConfig.PublisherHomePageUrl))
            {
                return new NotFoundResult();
            }
            else
            {
                return new RedirectResult(publisherConfig.PublisherHomePageUrl);
            }
        }

        protected SubscriptionOperationType ToCoreOperationType(string mpActionType) =>
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

                _ => throw AnOperationTypeNotSupportedException(mpActionType)
            };

        protected Task PublishSubscriptionPurchasedEvent(Subscription subscription) => 
            deploymentConfig.EventVersion switch
            {
                EventVersions.V_2021_05_01 =>
                subscriptionEventPublisher.PublishEventAsync(
                    new Events.V_2021_05_01.SubscriptionPurchased(subscription)),

                EventVersions.V_2021_10_01 =>
                subscriptionEventPublisher.PublishEventAsync(
                        new Events.V_2021_10_01.SubscriptionPurchased(subscription)),

                _ => throw AnEventVersionNotSupportedException()
            };

        protected Task PublishWebhookSubscriptionEvent(
            SubscriptionOperationType opType, Subscription subscription, WebhookNotification whNotification) =>
            deploymentConfig.EventVersion switch
            {
                EventVersions.V_2021_05_01 =>
                PublishSubscriptionEvent_V_2021_05_01(opType, subscription, whNotification),

                EventVersions.V_2021_10_01 =>
                PublishSubscriptionEvent_V_2021_10_01(opType, subscription, whNotification),

                _ => throw AnEventVersionNotSupportedException()
            };

        private Task PublishSubscriptionEvent_V_2021_05_01(
            SubscriptionOperationType opType, Subscription subscription, WebhookNotification whNotification) =>
            opType switch
            {
                SubscriptionOperationType.Cancel =>
                subscriptionEventPublisher.PublishEventAsync(
                    new Events.V_2021_05_01.SubscriptionCanceled(subscription, whNotification.OperationId)),

                SubscriptionOperationType.ChangePlan =>
                subscriptionEventPublisher.PublishEventAsync(
                    new Events.V_2021_05_01.SubscriptionPlanChanged(subscription, whNotification.OperationId, whNotification.PlanId)),

                SubscriptionOperationType.ChangeSeatQuantity =>
                subscriptionEventPublisher.PublishEventAsync(
                    new Events.V_2021_05_01.SubscriptionSeatQuantityChanged(subscription, whNotification.OperationId, whNotification.SeatQuantity)),

                SubscriptionOperationType.Reinstate =>
                subscriptionEventPublisher.PublishEventAsync(
                    new Events.V_2021_05_01.SubscriptionReinstated(subscription, whNotification.OperationId)),

                SubscriptionOperationType.Suspend =>
                subscriptionEventPublisher.PublishEventAsync(
                    new Events.V_2021_05_01.SubscriptionSuspended(subscription, whNotification.OperationId)),

                SubscriptionOperationType.Renew =>
                subscriptionEventPublisher.PublishEventAsync(
                    new Events.V_2021_05_01.SubscriptionRenewed(subscription, whNotification.OperationId)),

                _ => throw AnOperationTypeNotSupportedException(opType)
            };

        private Task PublishSubscriptionEvent_V_2021_10_01(SubscriptionOperationType opType, Subscription subscription, WebhookNotification whNotification) =>
           opType switch
           {
               SubscriptionOperationType.Cancel =>
               subscriptionEventPublisher.PublishEventAsync(
                   new Events.V_2021_10_01.SubscriptionCanceled(subscription, whNotification.OperationId)),

               SubscriptionOperationType.ChangePlan =>
               subscriptionEventPublisher.PublishEventAsync(
                   new Events.V_2021_10_01.SubscriptionPlanChanged(subscription, whNotification.OperationId, whNotification.PlanId)),

               SubscriptionOperationType.ChangeSeatQuantity =>
               subscriptionEventPublisher.PublishEventAsync(
                   new Events.V_2021_10_01.SubscriptionSeatQuantityChanged(subscription, whNotification.OperationId, whNotification.SeatQuantity)),

               SubscriptionOperationType.Reinstate =>
               subscriptionEventPublisher.PublishEventAsync(
                   new Events.V_2021_10_01.SubscriptionReinstated(subscription, whNotification.OperationId)),

               SubscriptionOperationType.Suspend =>
               subscriptionEventPublisher.PublishEventAsync(
                   new Events.V_2021_10_01.SubscriptionSuspended(subscription, whNotification.OperationId)),

               SubscriptionOperationType.Renew =>
               subscriptionEventPublisher.PublishEventAsync(
                   new Events.V_2021_10_01.SubscriptionRenewed(subscription, whNotification.OperationId)),

               _ => throw AnOperationTypeNotSupportedException(opType)
           };

        private NotSupportedException AnEventVersionNotSupportedException() =>
            new NotSupportedException($"Subscription event version [{deploymentConfig.EventVersion}] not supported");

        private NotSupportedException AnOperationTypeNotSupportedException(object opType) =>
            new NotSupportedException($"Subscription operation type [{opType}] not supported.");
    }
}
