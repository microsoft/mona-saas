// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Models.MarketplaceAPI.V_2018_08_31
{
    using Newtonsoft.Json;

    public class MarketplaceUser
    {
        [JsonProperty("emailId")]
        public string EmailId { get; set; }

        [JsonProperty("objectId")]
        public string ObjectId { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        [JsonProperty("puid")]
        public string Puid { get; set; }
    }
}
