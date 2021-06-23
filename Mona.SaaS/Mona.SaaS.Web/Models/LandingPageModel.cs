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