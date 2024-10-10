// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Models.MarketplaceAPI.V_2018_08_31
{
    using Newtonsoft.Json;
    using System.Text.Json.Serialization;

    public class MarketplaceUser
    {
        [JsonProperty("emailId")]
        [JsonPropertyName("emailId")]
        public string EmailId { get; set; }

        [JsonProperty("objectId")]
        [JsonPropertyName("objectId")]
        public string ObjectId { get; set; }

        [JsonProperty("tenantId")]
        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; }

        [JsonProperty("puid")]
        [JsonPropertyName("puid")]
        public string Puid { get; set; }
    }
}
