// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Mona.SaaS.Core.Models
{
    /// <summary>
    /// Represents an Azure Marketplace user.
    /// </summary>
    public class MarketplaceUser
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("userEmail")]
        public string UserEmail { get; set; }

        [JsonProperty("aadObjectId")]
        public string AadObjectId { get; set; }

        [JsonProperty("aadTenantId")]
        public string AadTenantId { get; set; }
    }
}