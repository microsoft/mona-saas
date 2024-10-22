// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Web.Models
{
    public class LandingPageModel
    {
        public LandingPageModel() { }

        public LandingPageModel(bool inTestMode)
        {
            InTestMode = inTestMode;
        }

        public string SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public string OfferId { get; set; }
        public string PlanId { get; set; }
        public string PublisherDisplayName { get; set; }
        public string PublisherCopyrightNotice { get; set; }
        public string BeneficiaryEmailAddress { get; set; }
        public string PurchaserEmailAddress { get; set; }

        // For various landing page links...
        // The landing page should be intelligent enough to only show links when the URL is included in this model.

        public string PublisherHomePageUrl { get; set; }
        public string PublisherPrivacyNoticePageUrl { get; set; }
        public string PublisherContactPageUrl { get; set; }

        // View should look at this error code and, if one is provided, show an appropriate error message to the user.

        public string ErrorCode { get; set; }
        public string UserFriendlyName { get; set; }

        public bool InTestMode { get; set; }
        public bool IsFreeTrial { get; set; }

        public int? SeatQuantity { get; set; }
    }
}