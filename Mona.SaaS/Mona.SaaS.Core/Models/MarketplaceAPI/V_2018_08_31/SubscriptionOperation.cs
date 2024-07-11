// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Models.MarketplaceAPI.V_2018_08_31
{
    using Newtonsoft.Json;
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a subscription operation as defined by the Marketplace API.
    /// </summary>
    public class SubscriptionOperation
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("activityId")]
        [JsonPropertyName("activityId")]
        public string ActivityId { get; set; }

        [JsonProperty("subscriptionId")]
        [JsonPropertyName("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("offerId")]
        [JsonPropertyName("offerId")]
        public string OfferId { get; set; }

        [JsonProperty("publisherId")]
        [JsonPropertyName("publisherId")]
        public string PublisherId { get; set; }

        [JsonProperty("planId")]
        [JsonPropertyName("planId")]
        public string PlanId { get; set; }

        [JsonProperty("quantity")]
        [JsonPropertyName("quantity")]
        public int? Quantity { get; set; }

        [JsonProperty("action")]
        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonProperty("timeStamp")]
        [JsonPropertyName("timeStamp")]
        public DateTime? TimeStamp { get; set; }

        [JsonProperty("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonProperty("errorStatusCode")]
        [JsonPropertyName("errorStatusCode")]
        public string ErrorStatusCode { get; set; }

        [JsonProperty("errorMessage")]
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
