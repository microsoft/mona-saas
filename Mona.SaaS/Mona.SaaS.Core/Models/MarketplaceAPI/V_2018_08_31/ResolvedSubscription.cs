// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Models.MarketplaceAPI.V_2018_08_31
{
    using Newtonsoft.Json;
    using System.Text.Json.Serialization;

    public class ResolvedSubscription
    {
        [JsonProperty("subscription")]
        [JsonPropertyName("subscription")]
        public Subscription Subscription { get; set; }
    }
}
