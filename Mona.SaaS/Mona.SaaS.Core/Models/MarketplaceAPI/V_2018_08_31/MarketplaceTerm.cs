// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Models.MarketplaceAPI.V_2018_08_31
{
    using Newtonsoft.Json;
    using System;
    using System.Text.Json.Serialization;

    public class MarketplaceTerm
    {
        [JsonProperty("termUnit")]
        [JsonPropertyName("termUnit")]
        public string TermUnit { get; set; }

        [JsonProperty("startDate")]
        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("endDate")]
        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }
    }
}
