using Mona.SaaS.Web.Models.Admin;

namespace Mona.SaaS.Web.Models
{
    public class AdminPageViewData
    {
        public bool IsSetupComplete { get; set; }

        public string? DeploymentName { get; set; }
        public string? EventGridTopicName { get; set; }
        public string? EventGridTopicOverviewUrl { get; set; }
        public string? ResourceGroupOverviewUrl { get; set; }
        public string? ExternalIdentityName { get; set; }
        public string? InternalIdentityName { get; set; }
        public string? ExternalIdentityOverviewUrl { get; set; }
        public string? InternalIdentityOverviewUrl { get; set; }
        public string? AzureResourceGroupName { get; set; }
        public string? AzureSubscriptionId { get; set; }
        public string? AdminAppName { get; set; }
        public string? AdminAppSettingsUrl { get; set; }
        public string? CustomerAppName { get; set; }
        public string? CustomerAppSettingsUrl { get; set; }

        public string? TestLandingPageUrl { get; set; }
        public string? TestWebhookUrl { get; set; }

        public string? MonaVersion { get; set; }

        public string? UserFriendlyName { get; set; }

        public PartnerCenterTechnicalDetails? PartnerCenterTechnicalDetails { get; set; }
    }
}
