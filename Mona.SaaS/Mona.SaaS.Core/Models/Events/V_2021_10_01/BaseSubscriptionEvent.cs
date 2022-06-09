// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Constants;
using Mona.SaaS.Core.Interfaces;
using Newtonsoft.Json;
using System;

namespace Mona.SaaS.Core.Models.Events.V_2021_10_01
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

            Subscription = new FlatSubscription(subscription);
            SubscriptionId = subscription.SubscriptionId;
            OperationId = operationId ?? Guid.NewGuid().ToString();
            OperationDateTimeUtc = operationDateTimeUtc ?? DateTime.UtcNow;
        }

        [JsonProperty("Event ID")]
        public string EventId { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("Event Type")]
        public string EventType { get; set; }

        [JsonProperty("Event Version")]
        public string EventVersion { get; set; } = EventVersions.V_2021_10_01;

        [JsonProperty("Operation ID")]
        public string OperationId { get; set; }

        [JsonProperty("Subscription ID")]
        public string SubscriptionId { get; set; }

        [JsonProperty("Subscription")]
        public FlatSubscription Subscription { get; set; }

        [JsonProperty("Operation Date/Time UTC")]
        public DateTime OperationDateTimeUtc { get; set; } = DateTime.UtcNow;
    }
}