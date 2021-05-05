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

namespace Mona.SaaS.Web.Models.Admin
{
    /// <summary>
    /// Provides technical details to be used when registering the SaaS application with the Partner Center for Marketplace/AppSource publishing.
    /// </summary>
    /// <remarks>
    /// See [https://docs.microsoft.com/en-us/azure/marketplace/create-new-saas-offer-technical] for more information.
    /// </remarks>
    public class PartnerCenterTechnicalDetails
    {
        /// <summary>
        /// Gets/sets the URL that customers will land on after purchasing the SaaS application.
        /// </summary>
        public string LandingPageUrl { get; set; }

        /// <summary>
        /// Gets/sets the URL that Microsoft will call with subscription status updates.
        /// </summary>
        public string WebhookUrl { get; set; }

        /// <summary>
        /// Gets/sets the Azure Active Directory (AAD) tenant ID that this Mona deployment is registered under.
        /// </summary>
        public string AadTenantId { get; set; }

        /// <summary>
        /// Gets/sets the Azure Active Directory (AAD) application/client ID that this Mona deployment is registered under.
        /// </summary>
        public string AadApplicationId { get; set; }
    }
}