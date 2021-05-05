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

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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

namespace Mona.SaaS.Services.Default
{
    public class BlobStorageSubscriptionRepository : ISubscriptionRepository
    {
        private readonly BlobContainerClient containerClient;
        private readonly ILogger logger;

        public BlobStorageSubscriptionRepository(
            IOptionsSnapshot<Configuration> configSnapshot,
            ILogger<BlobStorageSubscriptionRepository> logger)
        {
            var config = configSnapshot.Value;
            var serviceClient = new BlobServiceClient(config.ConnectionString);

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
            public string ConnectionString { get; set; }

            [Required]
            public string ContainerName { get; set; }
        }
    }
}