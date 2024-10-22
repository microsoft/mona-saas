// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Interfaces
{
    using Mona.SaaS.Core.Models;
    using System.Threading.Tasks;

    public interface ISubscriptionStagingCache : IHealthCheckable
    {
        /// <summary>
        /// Puts/stages a <see cref="Subscription"/> into the staging repository.
        /// </summary>
        /// <param name="subscription">The <see cref="Subscription"/> to put/stage.</param>
        /// <returns>A bearer URL which provides time-limited, scoped access to the staged <see cref="Subscription"/>.</returns>
        Task<string> PutSubscriptionAsync(Subscription subscription); 
    }
}
