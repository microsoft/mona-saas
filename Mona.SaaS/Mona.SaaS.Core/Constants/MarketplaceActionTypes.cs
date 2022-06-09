// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Constants
{
    /// <summary>
    /// Represents different subscription action types accepted by the Marketplace SaaS app webhook.
    /// See https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#implementing-a-webhook-on-the-saas-service for more information.
    /// </summary>
    public static class MarketplaceActionTypes
    {
        public const string ChangePlan = "ChangePlan";
        public const string ChangeQuantity = "ChangeQuantity";
        public const string Suspend = "Suspend";
        public const string Reinstate = "Reinstate";
        public const string Unsubscribe = "Unsubscribe";
        public const string Renew = "Renew";
    }
}