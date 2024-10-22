// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Mona.SaaS.Core.Models
{
    /// <summary>
    /// Represents an Azure Marketplace subscription term.
    /// </summary>
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