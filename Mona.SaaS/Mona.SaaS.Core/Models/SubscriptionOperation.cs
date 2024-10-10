// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Enumerations;
using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Mona.SaaS.Core.Models
{
    /// <summary>
    /// Represents an asynchronous, trackable subscription operation (change plan, quantity, etc.) initiated by the Marketplace.
    /// </summary>
    public class SubscriptionOperation
    {
        [JsonProperty("subscriptionId")]
        [JsonPropertyName("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("operationId")]
        [JsonPropertyName("operationId")]
        public string OperationId { get; set; }

        [JsonProperty("planId")]
        [JsonPropertyName("planId")]
        public string PlanId { get; set; }

        [JsonProperty("seatQuantity")]
        [JsonPropertyName("seatQuantity")]
        public int? SeatQuantity { get; set; }

        [JsonProperty("operationType")]
        [JsonPropertyName("operationType")]
        public SubscriptionOperationType OperationType { get; set; }

        [JsonProperty("operationDateTimeUtc")]
        [JsonPropertyName("operationDateTimeUtc")]
        public DateTime OperationDateTimeUtc { get; set; }
    }
}