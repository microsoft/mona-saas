// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Constants
{
    /// <summary>
    /// Event types originating from the Azure Marketplace.
    /// </summary>
    public static class EventTypes
    {
        public const string SubscriptionPurchased = "Mona.SaaS.Marketplace.SubscriptionPurchased";
        public const string SubscriptionPlanChanged = "Mona.SaaS.Marketplace.SubscriptionPlanChanged";
        public const string SubscriptionSeatQuantityChanged = "Mona.SaaS.Marketplace.SubscriptionSeatQuantityChanged";
        public const string SubscriptionSuspended = "Mona.SaaS.Marketplace.SubscriptionSuspended";
        public const string SubscriptionReinstated = "Mona.SaaS.Marketplace.SubscriptionReinstated";
        public const string SubscriptionCanceled = "Mona.SaaS.Marketplace.SubscriptionCancelled";
        public const string SubscriptionRenewed = "Mona.SaaS.Marketplace.SubscriptionRenewed";

        public const string CheckingHealth = "Mona.SaaS.CheckingHealth";
    }
}