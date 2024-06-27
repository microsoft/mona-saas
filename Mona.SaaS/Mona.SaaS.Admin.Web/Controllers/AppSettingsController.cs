// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Web.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Mona.SaaS.Core.Models.Configuration;
    using Mona.SaaS.Services.Default;
    using Newtonsoft.Json;
    using System.Text;

    [Authorize]
    public class AppSettingsController : Controller
    {
        private readonly BlobStorageSubscriptionStagingCache.Configuration blobStagingCacheConfig;
        private readonly BlobStorageSubscriptionTestingCache.Configuration blobTestingCacheConfig;
        private readonly DeploymentConfiguration deploymentConfig;
        private readonly EventGridSubscriptionEventPublisher.Configuration eventGridConfig;
        private readonly IdentityConfiguration identityConfig;
        private readonly BlobStoragePublisherConfigurationStore.Configuration pubConfigStoreConfig;

        public AppSettingsController(
            IOptionsSnapshot<BlobStorageSubscriptionStagingCache.Configuration> blobStagingCacheConfig,
            IOptionsSnapshot<BlobStorageSubscriptionTestingCache.Configuration> blobTestingCacheConfig,
            IOptionsSnapshot<DeploymentConfiguration> deploymentConfig,
            IOptionsSnapshot<EventGridSubscriptionEventPublisher.Configuration> eventGridConfig,
            IOptionsSnapshot<IdentityConfiguration> identityConfig,
            IOptionsSnapshot<BlobStoragePublisherConfigurationStore.Configuration> pubConfigStoreConfig)
        {
            this.blobStagingCacheConfig = blobStagingCacheConfig.Value;
            this.blobTestingCacheConfig = blobTestingCacheConfig.Value;
            this.deploymentConfig = deploymentConfig.Value;
            this.eventGridConfig = eventGridConfig.Value;
            this.identityConfig = identityConfig.Value;
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
