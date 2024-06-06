// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Services.Default
{
    using Azure.Core;
    using Azure.Identity;
    using Azure.Storage.Blobs;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Mona.SaaS.Core.Interfaces;
    using Mona.SaaS.Core.Models;
    using Mona.SaaS.Core.Models.Configuration;
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class BlobStorageSubscriptionStagingCache : ISubscriptionStagingCache
    {
        private readonly Configuration config;
        private readonly BlobContainerClient containerClient;
        private readonly ILogger logger;

        public BlobStorageSubscriptionStagingCache(
            IOptionsSnapshot<Configuration> configSnapshot,
            IOptionsSnapshot<IdentityConfiguration> identityConfigSnapshot,
            ILogger<BlobStorageSubscriptionTestingCache> logger)
        {
            this.config = configSnapshot.Value;

            var identityConfig = identityConfigSnapshot.Value;
            var internalManagedId = new ResourceIdentifier(identityConfig.ManagedIdentities.InternalManagedId);

            var serviceClient = new BlobServiceClient(
                new Uri($"https://{config.StorageAccountName}.blob.core.windows.net"),
                new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityResourceId = internalManagedId }));

            this.containerClient = serviceClient.GetBlobContainerClient(config.ContainerName);
            this.logger = logger;
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                return await containerClient.ExistsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "An error occurred while attempting to check subscription staging cache health. " +
                    "For more details see exception.");

                return false;
            }
        }

        public async Task PutSubscriptionAsync(Subscription subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException(nameof(subscription));
            }

            try
            {
                var blobClient = containerClient.GetBlobClient(subscription.SubscriptionId);
                var expiryTime = DateTime.UtcNow.AddSeconds(this.config.TokenExpirationInSeconds);

                await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(subscription))), overwrite: true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    $"An error occurred while attempting to put subscription [{subscription.SubscriptionId}] into blob storage. " +
                    $"For more details see exception.");

                throw;
            }
        }

        public class Configuration
        {
            [Required]
            public string StorageAccountName { get; set; }

            public string ContainerName { get; set; } = "staged-subscriptions";

            public double TokenExpirationInSeconds { get; set; } = TimeSpan.FromMinutes(5).TotalSeconds;
        }
    }
}
