// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Mona.SaaS.Core.Models.Events
{
    public partial class FlatSubscription
    {
        [JsonProperty("Purchaser User ID")]
        public string PurchaserUserId { get; set; }

        [JsonProperty("Purchaser Email Address")]
        public string PurchaserEmailAddress { get; set; }

        [JsonProperty("Purchaser AAD Object ID")]
        public string PurchaserAadObjectId { get; set; }

        [JsonProperty("Purchaser AAD Tenant ID")]
        public string PurchaserAadTenantId { get; set; }

        private void ApplyPurchaserInfo(MarketplaceUser user)
        {
            if (user != null)
            {
                PurchaserUserId = user.UserId;
                PurchaserEmailAddress = user.UserEmail;
                PurchaserAadObjectId = user.AadObjectId;
                PurchaserAadTenantId = user.AadTenantId;
            }
        }
    }
}
