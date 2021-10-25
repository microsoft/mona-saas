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

using Newtonsoft.Json;
using System;

namespace Mona.SaaS.Core.Models.Events
{
    /// <summary>
    /// Represents a base subscription-level event.
    /// </summary>
    public abstract class BaseSubscriptionEvent
    {
        protected BaseSubscriptionEvent(string eventType, string eventVersion)
        {
            if (string.IsNullOrEmpty(eventType))
            {
                throw new ArgumentNullException(nameof(eventType));
            }

            if (string.IsNullOrEmpty(eventVersion))
            {
                throw new ArgumentNullException(nameof(eventVersion));
            }

            EventId = Guid.NewGuid().ToString();
            EventType = eventType;
            EventVersion = eventVersion;
        }

        public BaseSubscriptionEvent(string eventType, string eventVersion, Subscription subscription, string operationId, DateTime operationDateTimeUtc)
            : this(eventType, eventVersion)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException(nameof(subscription));
            }

            Subscription = new FlatSubscription(subscription);
            OperationId = operationId;
            OperationDateTimeUtc = operationDateTimeUtc;
        }

        [JsonProperty("Event ID")]
        public string EventId { get; set; }

        [JsonProperty("Event Type")]
        public string EventType { get; set; }

        [JsonProperty("Event Version")]
        public string EventVersion { get; set; }

        [JsonProperty("Operation ID")]
        public string OperationId { get; set; }

        [JsonProperty("Subscription")]
        public FlatSubscription Subscription { get; set; }

        [JsonProperty("Operation Date/Time UTC")]
        public DateTime OperationDateTimeUtc { get; set; }
    }
}