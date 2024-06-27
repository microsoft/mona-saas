// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Services
{
    using Azure.Core;
    using Azure.Identity;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Mona.SaaS.Core.Interfaces;
    using Mona.SaaS.Core.Models.Configuration;
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class BlobStoragePublisherConfigurationStore : IPublisherConfigurationStore
    {
        public const string ConfigurationBlobName = "publisher-config.json";

        private readonly Configuration config;
        private readonly BlobContainerClient containerClient;
        private readonly ILogger logger;

        public BlobStoragePublisherConfigurationStore(
            IOptionsSnapshot<Configuration> configSnapshot,
            IOptionsSnapshot<IdentityConfiguration> identityConfigSnapshot,
            ILogger<BlobStoragePublisherConfigurationStore> logger)
        {
            config = configSnapshot.Value;

            var identityConfig = identityConfigSnapshot.Value;
            var internalManagedId = new ResourceIdentifier(identityConfig.ManagedIdentities.InternalManagedId);

            var serviceClient = new BlobServiceClient(
                new Uri($"https://{config.StorageAccountName}.blob.core.windows.net"),
                new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityResourceId = internalManagedId }));

            containerClient = serviceClient.GetBlobContainerClient(config.ContainerName);
            this.logger = logger;
        }

        public async Task<PublisherConfiguration> GetPublisherConfiguration()
        {
            try
            {
                var blobClient = containerClient.GetBlobClient(ConfigurationBlobName);

                if (await blobClient.ExistsAsync()) // Has the publisher set up this Mona deployment yet?
                {
                    BlobDownloadInfo blobDownload = await blobClient.DownloadAsync();

                    using (var streamReader = new StreamReader(blobDownload.Content, Encoding.UTF8))
                    {
                        return JsonConvert.DeserializeObject<PublisherConfiguration>(streamReader.ReadToEnd());
                    }
                }
                else
                {
                    return null; // The publisher has not set up this Mona deployment yet. No big deal.
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while trying to get publisher configuration. See exception for more details.");

                throw;
            }
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                // Can we access the publisher configuration container?

                var publisherConfig = await GetPublisherConfiguration();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task PutPublisherConfiguration(PublisherConfiguration publisherConfig)
        {
            if (publisherConfig == null)
            {
                throw new ArgumentNullException(nameof(publisherConfig));
            }

            try
            {
                var blobClient = containerClient.GetBlobClient(ConfigurationBlobName);

                await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(publisherConfig))), overwrite: true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while trying to put publisher configuration. See exception for more details.");

                throw;
            }
        }

        public class Configuration
        {
            [Required]
            public string StorageAccountName { get; set; }

            public string ContainerName { get; set; } = "configuration";
        }
    }
}
