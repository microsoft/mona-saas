// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Models;
using System.Threading.Tasks;

namespace Mona.SaaS.Core.Interfaces
{
    public interface ISubscriptionTestingCache : IHealthCheckable
    {
        /// <summary>
        /// Gets a <see cref="Subscription"/> from the repository by its unique <paramref name="subscriptionId"/>.
        /// </summary>
        /// <param name="subscriptionId">The unique <see cref="Subscription"/> ID.</param>
        /// <returns>A <see cref="Task{Subscription}"/> representing the operation and resulting in the requested <see cref="Subscription"/> (if found).</returns>
        Task<Subscription> GetSubscriptionAsync(string subscriptionId);

        /// <summary>
        /// Puts a <see cref="Subscription"/> into the repository.
        /// </summary>
        /// <param name="subscription">The <see cref="Subscription"/> to put into the repository.</param>
        /// <returns>A <see cref="Task"/> representing the operation.</returns>
        Task PutSubscriptionAsync(Subscription subscription);
    }
}