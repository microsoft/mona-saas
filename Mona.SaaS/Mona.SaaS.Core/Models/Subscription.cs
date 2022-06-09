// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Enumerations;
using Newtonsoft.Json;

namespace Mona.SaaS.Core.Models
{
    /// <summary>
    /// Represents an Azure Marketplace SaaS subscription.
    /// </summary>
    public class Subscription
    {
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("subscriptionName")]
        public string SubscriptionName { get; set; }

        [JsonProperty("offerId")]
        public string OfferId { get; set; }

        [JsonProperty("planId")]
        public string PlanId { get; set; }

        [JsonProperty("isTest")]
        public bool IsTest { get; set; }

        [JsonProperty("isFreeTrial")]
        public bool IsFreeTrial { get; set; }

        [JsonProperty("seatQuantity")]
        public int? SeatQuantity { get; set; }

        [JsonProperty("status")]
        public SubscriptionStatus Status { get; set; }

        [JsonProperty("term")]
        public MarketplaceTerm Term { get; set; }

        [JsonProperty("beneficiary")]
        public MarketplaceUser Beneficiary { get; set; }

        [JsonProperty("purchaser")]
        public MarketplaceUser Purchaser { get; set; }
    }
}