// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.Web.Models;
using Mona.SaaS.Web.Models.Admin;

namespace Mona.SaaS.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly DeploymentConfiguration deploymentConfig;
        private readonly IdentityConfiguration identityConfig;
        private readonly MarketplaceConfiguration marketplaceConfig;
        private readonly ILogger logger;
        private readonly IPublisherConfigurationStore publisherConfigStore;

        public AdminController(
            IOptionsSnapshot<DeploymentConfiguration> deploymentConfig,
            IOptionsSnapshot<IdentityConfiguration> identityConfig,
            IOptionsSnapshot<MarketplaceConfiguration> marketplaceConfig,
            ILogger<AdminController> logger,
            IPublisherConfigurationStore publisherConfigStore)
        {
            this.deploymentConfig = deploymentConfig.Value;
            this.identityConfig = identityConfig.Value;
            this.marketplaceConfig = marketplaceConfig.Value;
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
                    MonaVersion = this.deploymentConfig.MonaVersion,
                    AzureSubscriptionId = this.deploymentConfig.AzureSubscriptionId,
                    AzureResourceGroupName = this.deploymentConfig.AzureResourceGroupName,
                    EventGridTopicOverviewUrl = GetEventGridTopicUrl(),
                    ConfigurationSettingsUrl = GetConfigurationSettingsEditorUrl(),
                    PartnerCenterTechnicalDetails = GetPartnerCenterTechnicalDetails(),
                    ResourceGroupOverviewUrl = GetResourceGroupUrl(),
                    TestLandingPageUrl = Url.RouteUrl("landing/test", null, Request.Scheme)!,
                    TestWebhookUrl = Url.RouteUrl("webhook/test", null, Request.Scheme)!
                };

                return View(adminModel);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while attempting to load the Mona admin page. See exception for details.");

                throw;
            }
        }

        private string GetConfigurationSettingsEditorUrl() =>
            $"https://portal.azure.com/#@{this.identityConfig.EntraTenantId}/resource/subscriptions/{this.deploymentConfig.AzureSubscriptionId}" +
            $"/resourceGroups/{this.deploymentConfig.AzureResourceGroupName}/providers/Microsoft.Web" +
            $"/sites/mona-web-{this.deploymentConfig.Name.ToLower()}/configuration";

        private string GetEventGridTopicUrl() =>
            $"https://portal.azure.com/#@{this.identityConfig.EntraTenantId}/resource/subscriptions/{this.deploymentConfig.AzureSubscriptionId}" +
            $"/resourceGroups/{this.deploymentConfig.AzureResourceGroupName}/providers/Microsoft.EventGrid" +
            $"/topics/mona-events-{this.deploymentConfig.Name.ToLower()}/overview";

        private string GetResourceGroupUrl() =>
            $"https://portal.azure.com/#@{this.identityConfig.EntraTenantId}/resource/subscriptions/{this.deploymentConfig.AzureSubscriptionId}" +
            $"/resourceGroups/{this.deploymentConfig.AzureResourceGroupName}/overview";

        private PartnerCenterTechnicalDetails GetPartnerCenterTechnicalDetails() =>
            new PartnerCenterTechnicalDetails
            {
                AadApplicationId = this.identityConfig.ManagedIdentities.ExternalClientId,
                AadTenantId = this.identityConfig.EntraTenantId,
                LandingPageUrl = this.marketplaceConfig.LandingPageUrl,
                WebhookUrl = this.marketplaceConfig.WebhookUrl
            };
    }
}