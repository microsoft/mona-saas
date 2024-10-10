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
        /// <returns></returns>
        Task PutSubscriptionAsync(Subscription subscription); 
    }
}
