// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Admin.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Mona.SaaS.Core.Models.Configuration;
    using Mona.SaaS.Services;
    using Newtonsoft.Json;
    using System.Text;

    public class AppSettingsController : Controller
    {
        private readonly DeploymentConfiguration deploymentConfig;
        private readonly IdentityConfiguration identityConfig;
        private readonly MarketplaceConfiguration marketplaceConfig;
        private readonly BlobStorageSubscriptionStagingCache.Configuration blobStagingCacheConfig;
        private readonly BlobStorageSubscriptionTestingCache.Configuration blobTestingCacheConfig;
        private readonly EventGridSubscriptionEventPublisher.Configuration eventGridConfig;
        private readonly BlobStoragePublisherConfigurationStore.Configuration pubConfigStoreConfig;

        public AppSettingsController(
            IOptionsSnapshot<DeploymentConfiguration> deploymentConfig,
            IOptionsSnapshot<IdentityConfiguration> identityConfig,
            IOptionsSnapshot<MarketplaceConfiguration> marketplaceConfig,
            IOptionsSnapshot<BlobStorageSubscriptionStagingCache.Configuration> blobStagingCacheConfig,
            IOptionsSnapshot<BlobStorageSubscriptionTestingCache.Configuration> blobTestingCacheConfig,
            IOptionsSnapshot<EventGridSubscriptionEventPublisher.Configuration> eventGridConfig,
            IOptionsSnapshot<BlobStoragePublisherConfigurationStore.Configuration> pubConfigStoreConfig)
        {
            this.blobStagingCacheConfig = blobStagingCacheConfig.Value;
            this.blobTestingCacheConfig = blobTestingCacheConfig.Value;
            this.deploymentConfig = deploymentConfig.Value;
            this.eventGridConfig = eventGridConfig.Value;
            this.identityConfig = identityConfig.Value;
            this.marketplaceConfig = marketplaceConfig.Value;
            this.pubConfigStoreConfig = pubConfigStoreConfig.Value;
        }

        [HttpGet, Route("appsettings", Name = "appsettings")]
        public IActionResult GetAppSettings()
        {
            // Do some fancy JSON gymnastics and put this appsettings.json file together...

            var appSettings = new
            {
                AllowedHosts = "*",
                Logging = new { LogLevel = new { Default = "Information" } },
                Deployment = deploymentConfig,
                Identity = identityConfig,
                Marketplace = marketplaceConfig,
                PublisherConfig = new { Store = new { BlobStorage = pubConfigStoreConfig } },
                Subscriptions = new
                {
                    Events = new { EventGrid = eventGridConfig },
                    Staging = new { Cache = new { BlobStorage = blobStagingCacheConfig } },
                    Testing = new { Cache = new { BlobStorage = blobTestingCacheConfig } }
                }
            };

            // pretty it up so it's readable...

            var appSettingsJson = JsonConvert.SerializeObject(
                appSettings, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });

            // drop it into a byte array...

            var appSettingsBytes = Encoding.UTF8.GetBytes(appSettingsJson);

            // and download it to the user's browser. 

            return File(appSettingsBytes, "application/octet-stream", "appsettings.json");
        }
    }
}
