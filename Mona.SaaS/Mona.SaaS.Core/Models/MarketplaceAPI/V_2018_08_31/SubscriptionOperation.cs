// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Models.MarketplaceAPI.V_2018_08_31
{
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// Represents a subscription operation as defined by the Marketplace API.
    /// </summary>
    public class SubscriptionOperation
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("activityId")]
        public string ActivityId { get; set; }

        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("offerId")]
        public string OfferId { get; set; }

        [JsonProperty("publisherId")]
        public string PublisherId { get; set; }

        [JsonProperty("planId")]
        public string PlanId { get; set; }

        [JsonProperty("quantity")]
        public int? Quantity { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("timeStamp")]
        public DateTime? TimeStamp { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("errorStatusCode")]
        public string ErrorStatusCode { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
