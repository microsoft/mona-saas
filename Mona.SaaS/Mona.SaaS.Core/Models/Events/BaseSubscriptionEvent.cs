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

            EventType = eventType;
            EventVersion = eventVersion;
        }

        [JsonProperty("eventId")]
        public string EventId { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("eventVersion")]
        public string EventVersion { get; set; }

        [JsonProperty("operationId")]
        public string OperationId { get; set; }

        [JsonProperty("subscription")]
        public Subscription Subscription { get; set; }

        [JsonProperty("operationDateTimeUtc")]
        public DateTime OperationDateTimeUtc { get; set; } = DateTime.UtcNow;
    }
}