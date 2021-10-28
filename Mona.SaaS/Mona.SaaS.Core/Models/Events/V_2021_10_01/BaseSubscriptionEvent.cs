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