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