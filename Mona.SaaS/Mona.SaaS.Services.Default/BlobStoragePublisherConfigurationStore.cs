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

namespace Mona.SaaS.Services.Default
{
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
            ILogger<BlobStoragePublisherConfigurationStore> logger)
        {
            this.config = configSnapshot.Value;

            var serviceClient = new BlobServiceClient(this.config.ConnectionString);

            this.containerClient = serviceClient.GetBlobContainerClient(config.ContainerName);
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
            public string ConnectionString { get; set; }

            public string ContainerName { get; set; } = "configuration";
        }
    }
}
