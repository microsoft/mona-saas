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

using Mona.SaaS.Core.Models;
using System.Threading.Tasks;

namespace Mona.SaaS.Core.Interfaces
{
    /// <summary>
    /// Defines an interface for resolving Marketplace subscription metadata.
    /// </summary>
    public interface IMarketplaceSubscriptionService
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