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

using Mona.SaaS.Core.Models.Events.V_2021_10_01;
using System.Threading.Tasks;

namespace Mona.SaaS.Core.Interfaces
{
    /// <summary>
    /// Defines an interface for publishing subscription-related events.
    /// </summary>
    public interface ISubscriptionEventPublisher
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