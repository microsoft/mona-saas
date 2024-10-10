// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Identity;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Mona.SaaS.Services
{
    public class EventGridSubscriptionEventPublisher : ISubscriptionEventPublisher
    {
        private readonly ILogger logger;
        private readonly EventGridPublisherClient eventGridClient;
        private readonly string topicHostName;

        public EventGridSubscriptionEventPublisher(
            IOptionsSnapshot<IdentityConfiguration> identityConfigSnapshot,
            ILogger<EventGridSubscriptionEventPublisher> logger,
            IOptions<Configuration> optionsAccessor)
        {
            this.logger = logger;

            var options = optionsAccessor.Value;
            var identityConfig = identityConfigSnapshot.Value;
            var internalManagedId = new ResourceIdentifier(identityConfig.ManagedIdentities.InternalManagedId);
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityResourceId = internalManagedId });

            eventGridClient = new EventGridPublisherClient(new Uri(options.TopicEndpoint), credential);
            topicHostName = new Uri(options.TopicEndpoint).Host;
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var healthEvent = new EventGridEvent(
                    "mona/saas/health",
                    Core.Constants.EventTypes.CheckingHealth,
                    Core.Constants.EventTypes.CheckingHealth,
                    new { Message = "Please ignore. This is an automated health check event." });

                await eventGridClient.SendEventAsync(healthEvent);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "An error occurred while attempting to check event grid topic health. " +
                    "For more details see exception.");

                return false;
            }
        }

        /// <summary>
        /// Publishes the provided SubscriptionEvent to a custom Event Grid topic.
        /// </summary>
        /// <typeparam name="T">The type of subscription event.</typeparam>
        /// <param name="subscriptionEvent">The subscription event.</param>
        /// <returns></returns>
        public async Task PublishEventAsync<T>(T subscriptionEvent) where T : ISubscriptionEvent
        {
            if (subscriptionEvent == null)
            {
                throw new ArgumentNullException(nameof(subscriptionEvent));
            }

            try
            {
                var subEvent = new EventGridEvent(
                    $"mona/saas/subscriptions/{subscriptionEvent.SubscriptionId}",
                    subscriptionEvent.EventType,
                    subscriptionEvent.EventVersion,
                    subscriptionEvent);

                await eventGridClient.SendEventAsync(subEvent);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred while attempting to publish event [{subscriptionEvent.EventId}] to topic [{topicHostName}].");

                throw;
            }
        }

        public class Configuration
        {
            [Required]
            public string TopicEndpoint { get; set; }
        }
    }
}