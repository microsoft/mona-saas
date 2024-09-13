// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mona.SaaS.Admin.Web.Models.Admin;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.Web.Models;
using Mona.SaaS.Web.Models.Admin;

namespace Mona.SaaS.Admin.Web.Controllers
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
                var publisherConfig = await publisherConfigStore.GetPublisherConfiguration();

                if (publisherConfig == null) // No publisher config. Publisher needs to complete setup.
                {
                    return RedirectToRoute("setup");
                }

                var adminModel = new AdminPageModel
                {
                    IsSetupComplete = publisherConfig.IsSetupComplete,
                    MonaVersion = deploymentConfig.MonaVersion,
                    AzureSubscriptionId = deploymentConfig.AzureSubscriptionId,
                    AzureResourceGroupName = deploymentConfig.AzureResourceGroupName,
                    EventGridTopicOverviewUrl = GetEventGridTopicUrl(),
                    PartnerCenterTechnicalDetails = GetPartnerCenterTechnicalDetails(),
                    ResourceGroupOverviewUrl = GetResourceGroupUrl(),
                    TestLandingPageUrl = Url.RouteUrl("landing/test", null, Request.Scheme)!,
                    TestWebhookUrl = Url.RouteUrl("webhook/test", null, Request.Scheme)!,
                    UserRedirectionSettings = new UserRedirectionSettings(publisherConfig)
                };

                return View(adminModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while attempting to load the Mona admin page. See exception for details.");

                throw;
            }
        }

        private string GetConfigurationSettingsEditorUrl() =>
            $"https://portal.azure.com/#@{identityConfig.EntraTenantId}/resource/subscriptions/{deploymentConfig.AzureSubscriptionId}" +
            $"/resourceGroups/{deploymentConfig.AzureResourceGroupName}/providers/Microsoft.Web" +
            $"/sites/mona-web-{deploymentConfig.Name.ToLower()}/configuration";

        private string GetEventGridTopicUrl() =>
            $"https://portal.azure.com/#@{identityConfig.EntraTenantId}/resource/subscriptions/{deploymentConfig.AzureSubscriptionId}" +
            $"/resourceGroups/{deploymentConfig.AzureResourceGroupName}/providers/Microsoft.EventGrid" +
            $"/topics/mona-events-{deploymentConfig.Name.ToLower()}/overview";

        private string GetResourceGroupUrl() =>
            $"https://portal.azure.com/#@{identityConfig.EntraTenantId}/resource/subscriptions/{deploymentConfig.AzureSubscriptionId}" +
            $"/resourceGroups/{deploymentConfig.AzureResourceGroupName}/overview";

        private PartnerCenterTechnicalDetails GetPartnerCenterTechnicalDetails() =>
            new PartnerCenterTechnicalDetails
            {
                AadApplicationId = identityConfig.ManagedIdentities.ExternalClientId,
                AadTenantId = identityConfig.EntraTenantId,
                LandingPageUrl = marketplaceConfig.LandingPageUrl,
                WebhookUrl = marketplaceConfig.WebhookUrl
            };
    }
}