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

using Azure.Data.AppConfiguration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.Web.Models;
using System;
using System.Threading.Tasks;

namespace Mona.SaaS.Web.Controllers
{
    [Authorize(Policy = "admin")]
    public class SetupController : Controller
    {
        private readonly ILogger logger;
        private readonly OfferConfiguration offerConfig;

        public SetupController(
            ILogger<SetupController> logger,
            OfferConfiguration offerConfig)
        {
            this.logger = logger;
            this.offerConfig = offerConfig;
        }

        [HttpGet, Route("setup", Name = "setup")]
        public IActionResult Index()
        {
            if (this.offerConfig.IsSetupComplete)
            {
                return new RedirectToRouteResult("admin", null);
            }

            return View(new SetupModel());
        }

        [HttpPost, Route("setup", Name = "setup"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SetupModel setupModel)
        {
            if (ModelState.IsValid == false)
            {
                return View(setupModel);
            }

            var offerModel = setupModel.Offer;
            var configClient = new ConfigurationClient(GetAppConfigurationServiceConnectionString());

            Task.WaitAll(new[]
            {
                (configClient.SetConfigurationSettingAsync("Offer:OfferMarketplaceListingUrl", offerModel.OfferMarketplaceListingUrl)),
                (configClient.SetConfigurationSettingAsync("Offer:OfferMarketingPageUrl", offerModel.OfferMarketingPageUrl)),
                (configClient.SetConfigurationSettingAsync("Offer:OfferDisplayName", offerModel.OfferDisplayName)),
                (configClient.SetConfigurationSettingAsync("Offer:PublisherDisplayName", offerModel.PublisherDisplayName)),
                (configClient.SetConfigurationSettingAsync("Offer:PublisherHomePageUrl", offerModel.PublisherHomePageUrl)),
                (configClient.SetConfigurationSettingAsync("Offer:PublisherPrivacyNoticePageUrl", offerModel.PublisherPrivacyNoticePageUrl)),
                (configClient.SetConfigurationSettingAsync("Offer:PublisherContactPageUrl", offerModel.PublisherPrivacyNoticePageUrl)),
                (configClient.SetConfigurationSettingAsync("Offer:PublisherCopyrightNotice", offerModel.PublisherCopyrightNotice)),
                (configClient.SetConfigurationSettingAsync("Offer:SubscriptionConfigurationUrl", offerModel.SubscriptionConfigurationUrl)),
                (configClient.SetConfigurationSettingAsync("Offer:SubscriptionPurchaseConfirmationUrl", offerModel.SubscriptionPurchaseConfirmationUrl))
            });

            await configClient.SetConfigurationSettingAsync("Offer:IsSetupComplete", true.ToString());

            this.offerConfig.OfferDisplayName = offerModel.OfferDisplayName;
            this.offerConfig.OfferMarketplaceListingUrl = offerModel.OfferMarketplaceListingUrl;
            this.offerConfig.OfferMarketingPageUrl = offerModel.OfferMarketingPageUrl;
            this.offerConfig.PublisherDisplayName = offerModel.PublisherDisplayName;
            this.offerConfig.PublisherHomePageUrl = offerModel.PublisherHomePageUrl;
            this.offerConfig.PublisherPrivacyNoticePageUrl = offerModel.PublisherPrivacyNoticePageUrl;
            this.offerConfig.PublisherContactPageUrl = offerModel.PublisherContactPageUrl;
            this.offerConfig.PublisherCopyrightNotice = offerModel.PublisherCopyrightNotice;
            this.offerConfig.SubscriptionConfigurationUrl = offerModel.SubscriptionConfigurationUrl;
            this.offerConfig.SubscriptionPurchaseConfirmationUrl = offerModel.SubscriptionPurchaseConfirmationUrl;

            this.offerConfig.IsSetupComplete = true;

            return RedirectToRoute("admin");
        }

        private static string GetAppConfigurationServiceConnectionString() => Environment.GetEnvironmentVariable("APP_CONFIG_SERVICE_CONNECTION_STRING");
    }
}