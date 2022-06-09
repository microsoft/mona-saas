// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Enumerations
{
    /// <summary>
    /// Indicates the type of operation being performed on a subscription.
    /// </summary>
    public enum SubscriptionOperationType
    {
        Unknown = 0,
        Activate,
        ChangePlan,
        ChangeSeatQuantity,
        Reinstate,
        Suspend,
        Cancel,
        Renew
    }
}