// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Enumerations;
using Newtonsoft.Json;
using System;

namespace Mona.SaaS.Core.Models
{
    /// <summary>
    /// Represents an asynchronous, trackable subscription operation (change plan, quantity, etc.) initiated by the Marketplace.
    /// </summary>
    public class SubscriptionOperation
    {
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("operationId")]
        public string OperationId { get; set; }

        [JsonProperty("planId")]
        public string PlanId { get; set; }

        [JsonProperty("seatQuantity")]
        public int? SeatQuantity { get; set; }

        [JsonProperty("operationType")]
        public SubscriptionOperationType OperationType { get; set; }

        [JsonProperty("operationDateTimeUtc")]
        public DateTime OperationDateTimeUtc { get; set; }
    }
}