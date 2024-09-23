// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.Web.Models;
using Mona.SaaS.Web.Models.Admin;
using System.Security.Claims;

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

        private AdminPageViewData CreateViewData(PublisherConfiguration publisherConfig) =>
            new AdminPageViewData
            {
                DeploymentName = deploymentConfig.Name,
                IsSetupComplete = publisherConfig.IsSetupComplete,
                MonaVersion = deploymentConfig.MonaVersion,
                AzureSubscriptionId = deploymentConfig.AzureSubscriptionId,
                AzureResourceGroupName = deploymentConfig.AzureResourceGroupName,
                EventGridTopicName = $"mona-events-{deploymentConfig.Name.ToLower()}",
                ExternalIdentityName = $"mona-external-id-{deploymentConfig.Name.ToLower()}",
                InternalIdentityName = $"mona-internal-id-{deploymentConfig.Name.ToLower()}",
                AdminAppName = $"mona-admin-{deploymentConfig.Name.ToLower()}",
                CustomerAppName = $"mona-customer-{deploymentConfig.Name.ToLower()}",
                AdminAppSettingsUrl = GetAdminAppConfigurationSettingsEditorUrl(),
                CustomerAppSettingsUrl = GetCustomerAppConfigurationSettingsEditorUrl(),
                ExternalIdentityOverviewUrl = GetExternalIdentityOverviewUrl(),
                InternalIdentityOverviewUrl = GetInternalIdentityOverviewUrl(),
                EventGridTopicOverviewUrl = GetEventGridTopicUrl(),
                PartnerCenterTechnicalDetails = GetPartnerCenterTechnicalDetails(),
                ResourceGroupOverviewUrl = GetResourceGroupUrl(),
                TestLandingPageUrl = Url.RouteUrl("landing/test", null, Request.Scheme)!,
                TestWebhookUrl = Url.RouteUrl("webhook/test", null, Request.Scheme)!,
                UserFriendlyName = GetUserFriendlyName()
            };

        [HttpGet, Route("/", Name = "admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var publisherConfig = await publisherConfigStore.GetPublisherConfiguration()
                    ?? new PublisherConfiguration();

                ViewData["admin"] = CreateViewData(publisherConfig);

                return View(new AdminPageModel(publisherConfig));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while attempting to load the Mona admin page. See exception for details.");

                throw;
            }
        }

        [HttpPost, Route("/", Name = "admin")]
        public async Task<IActionResult> Index([FromForm] AdminPageModel adminModel)
        {
            try
            {
                var publisherConfig = await publisherConfigStore.GetPublisherConfiguration()
                    ?? new PublisherConfiguration();

                if (ModelState.IsValid == false)
                {
                    ViewData["admin"] = CreateViewData(publisherConfig);

                    return View(adminModel);
                }

                publisherConfig.IsSetupComplete = true;
                publisherConfig.PublisherHomePageUrl = adminModel.SaaSHomePageUrl;
                publisherConfig.SubscriptionConfigurationUrl = adminModel.SubscriptionConfigurationUrl;
                publisherConfig.SubscriptionPurchaseConfirmationUrl = adminModel.SubscriptionLandingUrl;

                await publisherConfigStore.PutPublisherConfiguration(publisherConfig);

                ViewData["admin"] = CreateViewData(publisherConfig);

                return View(adminModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while attempting to save the Mona admin page settings. See exception for details.");

                throw;
            }
        }

        private string GetUserFriendlyName() =>
            User?.FindFirst("given_name")?.Value ??
            User?.FindFirst("name")?.Value ??
            "Admin";

        private string GetAdminAppConfigurationSettingsEditorUrl() =>
            GetConfigurationSettingsEditorUrl($"mona-admin-{deploymentConfig.Name.ToLower()}");

        private string GetCustomerAppConfigurationSettingsEditorUrl() =>
            GetConfigurationSettingsEditorUrl($"mona-customer-{deploymentConfig.Name.ToLower()}");

        private string GetConfigurationSettingsEditorUrl(string webAppName) =>
            $"https://portal.azure.com/#@{identityConfig.EntraTenantId}/resource/subscriptions/{deploymentConfig.AzureSubscriptionId}" +
            $"/resourceGroups/{deploymentConfig.AzureResourceGroupName}/providers/Microsoft.Web" +
            $"/sites/{webAppName}/environmentVariablesAppSettings";

        private string GetExternalIdentityOverviewUrl() =>
            GetIdentityOverviewUrl($"mona-external-id-{deploymentConfig.Name.ToLower()}");

        private string GetInternalIdentityOverviewUrl() =>
            GetIdentityOverviewUrl($"mona-internal-id-{deploymentConfig.Name.ToLower()}");

        private string GetIdentityOverviewUrl(string identityName) =>
            $"https://portal.azure.com/#@{identityConfig.EntraTenantId}/resource/subscriptions/{deploymentConfig.AzureSubscriptionId}" +
            $"/resourceGroups/{deploymentConfig.AzureResourceGroupName}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{identityName}/overview";

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