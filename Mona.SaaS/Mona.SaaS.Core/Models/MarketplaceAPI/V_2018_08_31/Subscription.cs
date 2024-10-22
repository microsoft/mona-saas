// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Models.MarketplaceAPI.V_2018_08_31
{
    using Newtonsoft.Json;
    using System.Text.Json.Serialization;

    public class Subscription
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonProperty("publisherId")]
        [JsonPropertyName("publisherId")]
        public string PublisherId { get; set; }

        [JsonProperty("offerId")]
        [JsonPropertyName("offerId")]
        public string OfferId { get; set; }

        [JsonProperty("planId")]
        [JsonPropertyName("planId")]
        public string PlanId { get; set; }

        [JsonProperty("quantity")]
        [JsonPropertyName("quantity")]
        public int? Quantity { get; set; }

        [JsonProperty("beneficiary")]
        [JsonPropertyName("beneficiary")]
        public MarketplaceUser Beneficiary { get; set; }

        [JsonProperty("purchaser")]
        [JsonPropertyName("purchaser")]
        public MarketplaceUser Purchaser { get; set; }

        [JsonProperty("isFreeTrial")]
        [JsonPropertyName("isFreeTrial")]
        public bool? IsFreeTrial { get; set; }

        [JsonProperty("isTest")]
        [JsonPropertyName("isTest")]
        public bool? IsTest { get; set; }

        [JsonProperty("saasSubscriptionStatus")]
        [JsonPropertyName("saasSubscriptionStatus")]
        public string Status { get; set; }

        [JsonProperty("term")]
        [JsonPropertyName("term")]
        public MarketplaceTerm Term { get; set; }
    }
}
