// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Constants;
using Mona.SaaS.Core.Interfaces;
using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Mona.SaaS.Core.Models.Events.V_2021_05_01
{
    /// <summary>
    /// Represents a base subscription-level event.
    /// </summary>
    public abstract class BaseSubscriptionEvent : ISubscriptionEvent
    {
        protected BaseSubscriptionEvent(string eventType)
        {
            if (string.IsNullOrEmpty(eventType))
            {
                throw new ArgumentNullException(nameof(eventType));
            }

            EventType = eventType;
        }

        public BaseSubscriptionEvent(string eventType, Subscription subscription, string operationId = null, DateTime? operationDateTimeUtc = null)
            : this(eventType)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException(nameof(subscription));
            }

            Subscription = subscription;
            SubscriptionId = subscription.SubscriptionId;
            OperationId = operationId ?? Guid.NewGuid().ToString();
            OperationDateTimeUtc = operationDateTimeUtc ?? DateTime.UtcNow;
        }

        [JsonProperty("eventId")]
        [JsonPropertyName("eventId")]
        public string EventId { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("eventType")]
        [JsonPropertyName("eventType")]
        public string EventType { get; set; }

        [JsonProperty("eventVersion")]
        [JsonPropertyName("eventVersion")]
        public string EventVersion { get; set; } = EventVersions.V_2021_05_01;

        [JsonProperty("operationId")]
        [JsonPropertyName("operationId")]
        public string OperationId { get; set; }

        [JsonProperty("subscriptionId")]
        [JsonPropertyName("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("subscription")]
        [JsonPropertyName("subscription")]
        public Subscription Subscription { get; set; }

        [JsonProperty("operationDateTimeUtc")]
        [JsonPropertyName("operationDateTimeUtc")]
        public DateTime OperationDateTimeUtc { get; set; } = DateTime.UtcNow;
    }
}