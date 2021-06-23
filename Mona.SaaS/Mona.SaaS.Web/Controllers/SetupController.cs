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
        private readonly PublisherConfiguration publisherConfig;

        public SetupController(
            ILogger<SetupController> logger,
            PublisherConfiguration publisherConfig)
        {
            this.logger = logger;
            this.publisherConfig = publisherConfig;
        }

        [HttpGet, Route("setup", Name = "setup")]
        public IActionResult Index()
        {
            if (this.publisherConfig.IsSetupComplete)
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

            var offerModel = setupModel.Publisher;
            var configClient = new ConfigurationClient(GetAppConfigurationServiceConnectionString());

            Task.WaitAll(new[]
            {
                (configClient.SetConfigurationSettingAsync("Publisher:PublisherDisplayName", offerModel.PublisherDisplayName)),
                (configClient.SetConfigurationSettingAsync("Publisher:PublisherHomePageUrl", offerModel.PublisherHomePageUrl)),
                (configClient.SetConfigurationSettingAsync("Publisher:PublisherPrivacyNoticePageUrl", offerModel.PublisherPrivacyNoticePageUrl)),
                (configClient.SetConfigurationSettingAsync("Publisher:PublisherContactPageUrl", offerModel.PublisherPrivacyNoticePageUrl)),
                (configClient.SetConfigurationSettingAsync("Publisher:PublisherCopyrightNotice", offerModel.PublisherCopyrightNotice)),
                (configClient.SetConfigurationSettingAsync("Publisher:SubscriptionConfigurationUrl", offerModel.SubscriptionConfigurationUrl)),
                (configClient.SetConfigurationSettingAsync("Publisher:SubscriptionPurchaseConfirmationUrl", offerModel.SubscriptionPurchaseConfirmationUrl))
            });

            await configClient.SetConfigurationSettingAsync("Publisher:IsSetupComplete", true.ToString());

            this.publisherConfig.PublisherDisplayName = offerModel.PublisherDisplayName;
            this.publisherConfig.PublisherHomePageUrl = offerModel.PublisherHomePageUrl;
            this.publisherConfig.PublisherPrivacyNoticePageUrl = offerModel.PublisherPrivacyNoticePageUrl;
            this.publisherConfig.PublisherContactPageUrl = offerModel.PublisherContactPageUrl;
            this.publisherConfig.PublisherCopyrightNotice = offerModel.PublisherCopyrightNotice;
            this.publisherConfig.SubscriptionConfigurationUrl = offerModel.SubscriptionConfigurationUrl;
            this.publisherConfig.SubscriptionPurchaseConfirmationUrl = offerModel.SubscriptionPurchaseConfirmationUrl;

            this.publisherConfig.IsSetupComplete = true;

            return RedirectToRoute("admin");
        }

        private static string GetAppConfigurationServiceConnectionString() => Environment.GetEnvironmentVariable("APP_CONFIG_SERVICE_CONNECTION_STRING");
    }
}