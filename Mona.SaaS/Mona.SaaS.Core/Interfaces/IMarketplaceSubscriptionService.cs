// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Models;
using System.Threading.Tasks;

namespace Mona.SaaS.Core.Interfaces
{
    /// <summary>
    /// Defines an interface for resolving Marketplace subscription metadata.
    /// </summary>
    public interface IMarketplaceSubscriptionService : IHealthCheckable
    {
        /// <summary>
        /// Requests a specific subscription's metadata from the Marketplace using a its <paramref name="subscriptionId"/>.
        /// </summary>
        /// <remarks>
        /// See [https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#get-subscription] for more information.
        /// </remarks>
        /// <param name="subscriptionId">The subscription's ID.</param>
        /// <returns>The [<see cref="Subscription"/>] (if found).</returns>
        Task<Subscription> GetSubscriptionAsync(string subscriptionId);

        /// <summary>
        /// Requests a specific subscription's metadata from the Marketplace using a Marketplace-supplied <paramref name="subscriptionToken"/>.
        /// </summary>
        /// <remarks>
        /// See [https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#resolve-a-purchased-subscription] for more information.
        /// </remarks>
        /// <param name="subscriptionToken">The token provided by the Marketplace.</param>
        /// <returns>The [<see cref="Subscription"/>] (if found).</returns>
        Task<Subscription> ResolveSubscriptionTokenAsync(string subscriptionToken);
    }
}