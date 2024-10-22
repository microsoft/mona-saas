// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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

namespace Mona.SaaS.Services
{
    public class BlobStorageSubscriptionTestingCache : ISubscriptionTestingCache
    {
        private readonly BlobContainerClient containerClient;
        private readonly ILogger logger;

        public BlobStorageSubscriptionTestingCache(
            IOptionsSnapshot<Configuration> configSnapshot,
            IOptionsSnapshot<IdentityConfiguration> identityConfigSnapshot,
            ILogger<BlobStorageSubscriptionTestingCache> logger)
        {
            var config = configSnapshot.Value;

            var identityConfig = identityConfigSnapshot.Value;
            var internalManagedId = new ResourceIdentifier(identityConfig.ManagedIdentities.InternalManagedId);

            var serviceClient = new BlobServiceClient(
                new Uri($"https://{config.StorageAccountName}.blob.core.windows.net"),
                new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityResourceId = internalManagedId }));

            containerClient = serviceClient.GetBlobContainerClient(config.ContainerName);

            this.logger = logger;
        }

        public async Task<Subscription> GetSubscriptionAsync(string subscriptionId)
        {
            if (string.IsNullOrEmpty(subscriptionId))
            {
                throw new ArgumentNullException(nameof(subscriptionId));
            }

            try
            {
                var blobClient = containerClient.GetBlobClient(subscriptionId);

                if (await blobClient.ExistsAsync()) // Do we know about this subscription?
                {
                    BlobDownloadInfo blobDownload = await blobClient.DownloadAsync();

                    using (var streamReader = new StreamReader(blobDownload.Content, Encoding.UTF8))
                    {
                        return JsonConvert.DeserializeObject<Subscription>(streamReader.ReadToEnd());
                    }
                }
                else
                {
                    return null; // We don't know about this subscription.
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    $"An error occurred while attempting to get subscription [{subscriptionId} from blob storage. " +
                    $"For more details see exception.");

                throw;
            }
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                // Can we access the testing blob storage container?

                var subscription = await GetSubscriptionAsync(Guid.NewGuid().ToString());

                return true;
            }
            catch
            {
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

            public string ContainerName { get; set; } = "test-subscriptions";
        }
    }
}