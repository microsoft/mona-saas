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