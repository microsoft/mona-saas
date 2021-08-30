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