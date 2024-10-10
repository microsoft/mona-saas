// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Models;
using System.Threading.Tasks;

namespace Mona.SaaS.Core.Interfaces
{
    /// <summary>
    /// Defines an interface for managing Marketplace subscription operations.
    /// </summary>
    public interface IMarketplaceOperationService : IHealthCheckable
    {
        /// <summary>
        /// Tries to get a subscription operation from the Marketplace.
        /// </summary>
        /// <remarks>
        /// See [https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#get-operation-status] for more information.
        /// </remarks>
        /// <param name="subscriptionId">The operation's subscription ID.</param>
        /// <param name="operationId">The operation ID.</param>
        /// <returns>The requested [<see cref="SubscriptionOperation"/>] (if found).</returns>
        Task<SubscriptionOperation> GetSubscriptionOperationAsync(string subscriptionId, string operationId);

        /// <summary>
        /// Tries to notify the Marketplace that an operation has been completed.
        /// </summary>
        /// <param name="subscriptionId">The operation's subscription ID</param>
        /// <param name="operationId">The operation ID</param>
        /// <returns></returns>
        Task ConfirmOperationComplete(string subscriptionId, string operationId);

        /// <summary>
        /// Tries to notify the Marketplace that an operation has failed.
        /// </summary>
        /// <param name="subscriptionId">The operation's subscription ID</param>
        /// <param name="operationId">The operation ID</param>
        /// <returns></returns>
        Task ConfirmOperationFailed(string subscriptionId, string operationId);
    }
}