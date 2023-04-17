// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mona.AutoIntegration.Interrogators;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.Web.Models;
using Mona.SaaS.Web.Models.Admin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Mona.SaaS.Web.Controllers
{
    [Authorize(Policy = "admin")]
    public class AdminController : Controller
    {
        private readonly DeploymentConfiguration deploymentConfig;
        private readonly IdentityConfiguration identityConfig;
        private readonly ILogger logger;
        private readonly IPublisherConfigurationStore publisherConfigStore;

        public AdminController(
            IOptionsSnapshot<DeploymentConfiguration> deploymentConfig,
            IOptionsSnapshot<IdentityConfiguration> identityConfig,
            ILogger<AdminController> logger,
            IPublisherConfigurationStore publisherConfigStore)
        {
            this.deploymentConfig = deploymentConfig.Value;
            this.identityConfig = identityConfig.Value;
            this.publisherConfigStore = publisherConfigStore;
            this.logger = logger;
        }

        [HttpGet, Route("admin", Name = "admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var publisherConfig = await this.publisherConfigStore.GetPublisherConfiguration();

                if (publisherConfig == null) // No publisher config. Publisher needs to complete setup.
                {
                    return RedirectToRoute("setup");
                }

                var adminModel = new AdminPageModel
                {
                    IsTestModeEnabled = this.deploymentConfig.IsTestModeEnabled,
                    MonaVersion = this.deploymentConfig.MonaVersion,
                    AzureSubscriptionId = this.deploymentConfig.AzureSubscriptionId,
                    AzureResourceGroupName = this.deploymentConfig.AzureResourceGroupName,
                    EventGridTopicOverviewUrl = GetEventGridTopicUrl(),
                    IntegrationPlugins = (await GetAvailableIntegrationPluginModels()).ToList(),
                    ConfigurationSettingsUrl = GetConfigurationSettingsEditorUrl(),
                    PartnerCenterTechnicalDetails = GetPartnerCenterTechnicalDetails(),
                    ResourceGroupOverviewUrl = GetResourceGroupUrl(),
                    TestLandingPageUrl = Url.RouteUrl("landing/test", null, Request.Scheme),
                    TestWebhookUrl = Url.RouteUrl("webhook/test", null, Request.Scheme),
                    UserManagementUrl = GetUserManagementUrl()
                };

                return View(adminModel);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while attempting to load the Mona admin page. See exception for details.");

                throw;
            }
        }

        private string GetUserManagementUrl() =>
            $"https://portal.azure.com/#blade/Microsoft_AAD_IAM/ManagedAppMenuBlade/Users" +
            $"/objectId/{this.identityConfig.AppIdentity.AadPrincipalId}" +
            $"/appId/{this.identityConfig.AppIdentity.AadClientId}";

        private string GetConfigurationSettingsEditorUrl() =>
            $"https://portal.azure.com/#@{this.identityConfig.AppIdentity.AadTenantId}/resource/subscriptions/{this.deploymentConfig.AzureSubscriptionId}" +
            $"/resourceGroups/{this.deploymentConfig.AzureResourceGroupName}/providers/Microsoft.Web" +
            $"/sites/mona-web-{this.deploymentConfig.Name.ToLower()}/configuration";

        private string GetEventGridTopicUrl() =>
            $"https://portal.azure.com/#@{this.identityConfig.AppIdentity.AadTenantId}/resource/subscriptions/{this.deploymentConfig.AzureSubscriptionId}" +
            $"/resourceGroups/{this.deploymentConfig.AzureResourceGroupName}/providers/Microsoft.EventGrid" +
            $"/topics/mona-events-{this.deploymentConfig.Name.ToLower()}/overview";

        private string GetResourceGroupUrl() =>
            $"https://portal.azure.com/#@{this.identityConfig.AppIdentity.AadTenantId}/resource/subscriptions/{this.deploymentConfig.AzureSubscriptionId}" +
            $"/resourceGroups/{this.deploymentConfig.AzureResourceGroupName}/overview";

        private PartnerCenterTechnicalDetails GetPartnerCenterTechnicalDetails()
        {
            var techDetails = new PartnerCenterTechnicalDetails 
            {
                LandingPageUrl = Url.RouteUrl("landing", null, Request.Scheme),
                WebhookUrl = Url.RouteUrl("webhook", null, Request.Scheme)
            };

            // We recently added support for a dedicated Marketplace identity per this GH issue --
            // https://github.com/microsoft/mona-saas/issues/109

            // Since we don't want to break existing users, we first check to see if a dedicated 
            // Marketplace identity is configured (`identityConfig.MarketplaceIdentity`). If not, 
            // we fall back to the app identity. However, users can manually configure a 
            // Marketplace identity so they should choose. We should write a FAQ about this...

            if (!string.IsNullOrEmpty(identityConfig.MarketplaceIdentity.AadClientId) &&
                !string.IsNullOrEmpty(identityConfig.MarketplaceIdentity.AadTenantId))
            {
                techDetails.AadTenantId = identityConfig.MarketplaceIdentity.AadTenantId;
                techDetails.AadApplicationId = identityConfig.MarketplaceIdentity.AadClientId;
            }
            else
            {
                techDetails.AadTenantId = identityConfig.AppIdentity.AadTenantId;
                techDetails.AadApplicationId = identityConfig.AppIdentity.AadClientId;
            }

            return techDetails;
        }

        private async Task<IEnumerable<PluginModel>> GetAvailableIntegrationPluginModels()
        {
            var locale = CultureInfo.CurrentUICulture.Name;

            var azCredentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(
                identityConfig.AppIdentity.AadClientId,
                identityConfig.AppIdentity.AadClientSecret,
                identityConfig.AppIdentity.AadTenantId,
                AzureEnvironment.AzureGlobalCloud);

            var logicAppInterrogator = new LogicAppPluginInterrogator();

            var plugins = await logicAppInterrogator.InterrogateResourceGroupAsync(
                azCredentials, this.deploymentConfig.AzureSubscriptionId, this.deploymentConfig.AzureResourceGroupName);

            var pluginModels = plugins
                .Select(p => new PluginModel
                {
                    Description = p.Description.GetLocalPropertyValue(locale),
                    DisplayName = p.DisplayName.GetLocalPropertyValue(locale),
                    EditorUrl = p.EditorUrl,
                    Id = p.Id,
                    ManagementUrl = p.ManagementUrl,
                    PluginType = p.PluginType,
                    Status = p.Status,
                    TriggerEventType = p.TriggerEventType,
                    TriggerEventVersion = p.TriggerEventVersion,
                    Version = p.Version
                })
                .ToList();

            return pluginModels;
        }
    }
}