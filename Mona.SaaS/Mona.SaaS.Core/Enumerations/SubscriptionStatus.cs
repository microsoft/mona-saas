// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Enumerations
{
    /// <summary>
    /// Indicates the status of a SaaS subscription.
    /// </summary>
    public enum SubscriptionStatus
    {
        /// <summary>
        /// Subscription state is unknown.
        /// </summary>
        /// <remarks>
        /// This should never happen in the real world. If it does, it's probably not good.
        /// </remarks>
        Unknown = 0,

        /// <summary>
        /// Subscription presumably purchased through AppSource/Marketplace but not yet confirmed by purchaser.
        /// </summary>
        PendingConfirmation,

        /// <summary>
        /// Subscription activation pending publisher configuration and setup.
        /// </summary>
        PendingActivation,

        /// <summary>
        /// Subscription is active.
        /// </summary>
        Active,

        /// <summary>
        /// Subscription has been suspended (typically for non-payment).
        /// </summary>
        Suspended,

        /// <summary>
        /// Subscription has been cancelled.
        /// </summary>
        Cancelled
    }
}