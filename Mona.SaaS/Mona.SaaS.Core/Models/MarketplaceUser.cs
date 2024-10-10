// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Mona.SaaS.Core.Models
{
    /// <summary>
    /// Represents an Azure Marketplace user.
    /// </summary>
    public class MarketplaceUser
    {
        [JsonProperty("userId")]
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonProperty("userEmail")]
        [JsonPropertyName("userEmail")]
        public string UserEmail { get; set; }

        [JsonProperty("aadObjectId")]
        [JsonPropertyName("aadObjectId")]
        public string AadObjectId { get; set; }

        [JsonProperty("aadTenantId")]
        [JsonPropertyName("aadTenantId")]
        public string AadTenantId { get; set; }
    }
}