// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Interfaces
{
    using Mona.SaaS.Core.Models.Configuration;
    using System.Threading.Tasks;

    public interface IPublisherConfigurationStore : IHealthCheckable
    {
        /// <summary>
        /// Gets the current <see cref="PublisherConfiguration"/> from the configuration store.
        /// </summary>
        /// <returns>The current <see cref="PublisherConfiguration"/></returns>
        Task<PublisherConfiguration> GetPublisherConfiguration();

        /// <summary>
        /// Puts the current <see cref="PublisherConfiguration"/> into the configuration store.
        /// </summary>
        /// <param name="publisherConfig">The current <see cref="PublisherConfiguration"/></param>
        /// <returns>A <see cref="Task"/> representing the put operation</returns>
        Task PutPublisherConfiguration(PublisherConfiguration publisherConfig);
    }
}
