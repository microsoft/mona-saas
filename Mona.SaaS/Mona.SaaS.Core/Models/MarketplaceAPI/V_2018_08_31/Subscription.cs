// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Models.MarketplaceAPI.V_2018_08_31
{
    using Newtonsoft.Json;

    public class Subscription
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("publisherId")]
        public string PublisherId { get; set; }

        [JsonProperty("offerId")]
        public string OfferId { get; set; }

        [JsonProperty("planId")]
        public string PlanId { get; set; }

        [JsonProperty("quantity")]
        public int? Quantity { get; set; }

        [JsonProperty("beneficiary")]
        public MarketplaceUser Beneficiary { get; set; }

        [JsonProperty("purchaser")]
        public MarketplaceUser Purchaser { get; set; }

        [JsonProperty("isFreeTrial")]
        public bool? IsFreeTrial { get; set; }

        [JsonProperty("isTest")]
        public bool? IsTest { get; set; }

        [JsonProperty("saasSubscriptionStatus")]
        public string Status { get; set; }

        [JsonProperty("term")]
        public MarketplaceTerm Term { get; set; }
    }
}
