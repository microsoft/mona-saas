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
    using Azure.Storage.Sas;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Mona.SaaS.Core.Interfaces;
    using Mona.SaaS.Core.Models;
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
            ILogger<BlobStorageSubscriptionTestingCache> logger)
        {
            this.config = configSnapshot.Value;

            var serviceClient = new BlobServiceClient(this.config.ConnectionString);

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

        public async Task<string> PutSubscriptionAsync(Subscription subscription)
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

                var sasBuilder = new BlobSasBuilder(BlobContainerSasPermissions.Read, expiryTime)
                {
                    BlobContainerName = this.config.ContainerName,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };

                var sasToken = blobClient.GenerateSasUri(sasBuilder).ToString();

                return sasToken.Substring(sasToken.IndexOf($"/{this.config.ContainerName.ToLower()}"));
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
            public string ConnectionString { get; set; }

            public string ContainerName { get; set; } = "staged-subscriptions";

            public double TokenExpirationInSeconds { get; set; } = TimeSpan.FromMinutes(5).TotalSeconds;
        }
    }
}
