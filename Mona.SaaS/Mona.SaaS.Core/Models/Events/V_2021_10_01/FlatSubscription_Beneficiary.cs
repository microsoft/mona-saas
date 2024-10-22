// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Mona.SaaS.Core.Models.Events
{
    public partial class FlatSubscription
    {
        [JsonProperty("Beneficiary User ID")]
        public string BeneficiaryUserId { get; set; }

        [JsonProperty("Beneficiary Email Address")]
        public string BeneficiaryEmailAddress { get; set; }

        [JsonProperty("Beneficiary AAD Object ID")]
        public string BeneficiaryAadObjectId { get; set; }

        [JsonProperty("Beneficiary AAD Tenant ID")]
        public string BeneficiaryAadTenantId { get; set; }

        private void ApplyBeneficiaryInfo(MarketplaceUser user)
        {
            if (user != null)
            {
                BeneficiaryUserId = user.UserId;
                BeneficiaryEmailAddress = user.UserEmail;
                BeneficiaryAadObjectId = user.AadObjectId;
                BeneficiaryAadTenantId = user.AadTenantId;
            }
        }
    }
}
