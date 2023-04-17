// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;

namespace Mona.SaaS.Core.Models.Configuration
{
    /// <summary>
    /// This Mona application's Marketplace identity Azure Active Directory (AAD) configuration information.
    /// </summary>
    public class MarketplaceIdentityConfiguration
    {
        /// <summary>
        /// Gets/sets the Marketplace client's AAD client ID.
        /// </summary>
        public string AadClientId { get; set; }

        /// <summary>
        /// Gets/sets the Marketplace client's AAD client secret.
        /// </summary>
        public string AadClientSecret { get; set; }

        /// <summary>
        /// Gets/sets the Marketplace client's AAD tenant ID.
        /// </summary>
        public string AadTenantId { get; set; }
    }
}