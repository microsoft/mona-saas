// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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