// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Enumerations;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Mona.SaaS.Core.Models
{
    /// <summary>
    /// Represents an Azure Marketplace SaaS subscription.
    /// </summary>
    public class Subscription
    {
        [JsonProperty("subscriptionId")]
        [JsonPropertyName("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("subscriptionName")]
        [JsonPropertyName("subscriptionName")]
        public string SubscriptionName { get; set; }

        [JsonProperty("offerId")]
        [JsonPropertyName("offerId")]
        public string OfferId { get; set; }

        [JsonProperty("planId")]
        [JsonPropertyName("planId")]
        public string PlanId { get; set; }

        [JsonProperty("isTest")]
        [JsonPropertyName("isTest")]
        public bool IsTest { get; set; }

        [JsonProperty("isFreeTrial")]
        [JsonPropertyName("isFreeTrial")]
        public bool IsFreeTrial { get; set; }

        [JsonProperty("seatQuantity", NullValueHandling = NullValueHandling.Include)]
        [JsonPropertyName("seatQuantity")]
        public int? SeatQuantity { get; set; }

        [JsonProperty("status")]
        [JsonPropertyName("status")]
        public SubscriptionStatus Status { get; set; }

        [JsonProperty("term")]
        [JsonPropertyName("term")]
        public MarketplaceTerm Term { get; set; }

        [JsonProperty("beneficiary")]
        [JsonPropertyName("beneficiary")]
        public MarketplaceUser Beneficiary { get; set; }

        [JsonProperty("purchaser")]
        [JsonPropertyName("purchaser")]
        public MarketplaceUser Purchaser { get; set; }
    }
}