// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Models.Events.V_2021_10_01;
using System.Threading.Tasks;

namespace Mona.SaaS.Core.Interfaces
{
    /// <summary>
    /// Defines an interface for publishing subscription-related events.
    /// </summary>
    public interface ISubscriptionEventPublisher : IHealthCheckable
    {
        /// <summary>
        /// Publishes a subscription-related event.
        /// </summary>
        /// <typeparam name="T">The type of event to publish.</typeparam>
        /// <param name="subscriptionEvent">The event to publish.</param>
        /// <returns></returns>
        Task PublishEventAsync<T>(T subscriptionEvent) where T : ISubscriptionEvent;
    }
}