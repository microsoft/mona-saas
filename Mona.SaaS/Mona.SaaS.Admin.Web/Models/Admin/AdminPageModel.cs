// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Web.Models.Admin;

namespace Mona.SaaS.Web.Models
{
    public class AdminPageModel
    {
        public string ConfigurationSettingsUrl { get; set; }
        public string EventGridTopicOverviewUrl { get; set; }
        public string ResourceGroupOverviewUrl { get; set; }
        public string TestLandingPageUrl { get; set; }
        public string TestWebhookUrl { get; set; }

        public string MonaVersion { get; set; }
        public string AzureResourceGroupName { get; set; }
        public string AzureSubscriptionId { get; set; }

        public PartnerCenterTechnicalDetails PartnerCenterTechnicalDetails { get; set; }
    }
}