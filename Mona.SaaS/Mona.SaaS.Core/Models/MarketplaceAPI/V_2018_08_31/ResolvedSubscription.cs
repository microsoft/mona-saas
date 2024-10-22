// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Models.MarketplaceAPI.V_2018_08_31
{
    using Newtonsoft.Json;

    public class ResolvedSubscription
    {
        [JsonProperty("subscription")]
        public Subscription Subscription { get; set; }
    }
}
