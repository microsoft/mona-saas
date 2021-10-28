// MICROSOFT CONFIDENTIAL INFORMATION
//
// Copyright © Microsoft Corporation
//
// Microsoft Corporation (or based on where you live, one of its affiliates) licenses this preview code for your internal testing purposes only.
//
// Microsoft provides the following preview code AS IS without warranty of any kind. The preview code is not supported under any Microsoft standard support program or services.
//
// Microsoft further disclaims all implied warranties including, without limitation, any implied warranties of merchantability or of fitness for a particular purpose. The entire risk arising out of the use or performance of the preview code remains with you.
//
// In no event shall Microsoft be liable for any damages whatsoever (including, without limitation, damages for loss of business profits, business interruption, loss of business information, or other pecuniary loss) arising out of the use of or inability to use the preview code, even if Microsoft has been advised of the possibility of such damages.

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
