// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mona.SaaS.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Mona.SaaS.Services.Default
{
    public class EventGridSubscriptionEventPublisher : ISubscriptionEventPublisher
    {
        private readonly ILogger logger;
        private readonly EventGridClient eventGridClient;
        private readonly string topicHostName;

        public EventGridSubscriptionEventPublisher(
            ILogger<EventGridSubscriptionEventPublisher> logger,
            IOptions<Configuration> optionsAccessor)
        {
            this.logger = logger;

            var options = optionsAccessor.Value;

            eventGridClient = new EventGridClient(new TopicCredentials(options.TopicKey));
            topicHostName = new Uri(options.TopicEndpoint).Host;
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var healthEvent = new EventGridEvent(
                    Guid.NewGuid().ToString(),
                    "mona/saas/health",
                    new { Message = "Please ignore. This is an automated health check event." },
                    Core.Constants.EventTypes.CheckingHealth,
                    DateTime.UtcNow,
                    Core.Constants.EventTypes.CheckingHealth);

                await eventGridClient.PublishEventsAsync(topicHostName, new List<EventGridEvent> { healthEvent });

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
                var eventGridEvent = new EventGridEvent(
                    subscriptionEvent.EventId,
                    $"mona/saas/subscriptions/{subscriptionEvent.SubscriptionId}",
                    subscriptionEvent,
                    subscriptionEvent.EventType,
                    DateTime.UtcNow,
                    subscriptionEvent.EventVersion);

                await eventGridClient.PublishEventsAsync(topicHostName, new List<EventGridEvent> { eventGridEvent });
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

            [Required]
            public string TopicKey { get; set; }
        }
    }
}