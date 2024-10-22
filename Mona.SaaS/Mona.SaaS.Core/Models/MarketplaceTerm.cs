// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;

namespace Mona.SaaS.Core.Models
{
    /// <summary>
    /// Represents an Azure Marketplace subscription term.
    /// </summary>
    public class MarketplaceTerm
    {
        [JsonProperty("termUnit")]
        public string TermUnit { get; set; }

        [JsonProperty("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime? EndDate { get; set; }
    }
}