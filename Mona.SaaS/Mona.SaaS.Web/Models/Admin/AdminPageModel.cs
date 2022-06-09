// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Web.Models.Admin;
using System.Collections.Generic;

namespace Mona.SaaS.Web.Models
{
    public class AdminPageModel
    {
        public bool IsTestModeEnabled { get; set; }

        public string ConfigurationSettingsUrl { get; set; }
        public string EventGridTopicOverviewUrl { get; set; }
        public string ResourceGroupOverviewUrl { get; set; }
        public string TestLandingPageUrl { get; set; }
        public string TestWebhookUrl { get; set; }
        public string UserManagementUrl { get; set; }

        public string MonaVersion { get; set; }
        public string AzureResourceGroupName { get; set; }
        public string AzureSubscriptionId { get; set; }

        public List<PluginModel> IntegrationPlugins { get; set; } = new List<PluginModel>();

        public PartnerCenterTechnicalDetails PartnerCenterTechnicalDetails { get; set; }
    }
}