// MICROSOFT CONFIDENTIAL INFORMATION
//
// Copyright © Microsoft Corporation
//
// Microsoft Corporation (or based on where you live, one of its affiliates) licenses this preview code for your internal testing purposes only.
//
// Microsoft provides the following preview code AS IS without warranty of any kind. The preview code is not supported under any Microsoft standard support program or services.
//
// Microsoft further disclaims all implied warranties including, without limitation, any implied warranties of merchantability or of fitness for a particular purpose. The entire risk arising out of the use or performance of the preview code remains with you.
//
// In no event shall Microsoft be liable for any damages whatsoever (including, without limitation, damages for loss of business profits, business interruption, loss of business information, or other pecuniary loss) arising out of the use of or inability to use the preview code, even if Microsoft has been advised of the possibility of such damages.

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