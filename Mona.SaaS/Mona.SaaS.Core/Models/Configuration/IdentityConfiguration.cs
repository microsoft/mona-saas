// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;

namespace Mona.SaaS.Core.Models.Configuration
{
    /// <summary>
    /// Provides Azure Active Directory (AAD) identity information about this Mona application.
    /// </summary>
    public class IdentityConfiguration
    {
        /// <summary>
        /// Gets/sets information about this Mona application's administrative AAD identity.
        /// </summary>
        [Required]
        public AppAdminIdentityConfiguration AdminIdentity { get; set; }

        /// <summary>
        /// Gets/sets information about this Mona application's AAD identity.
        /// </summary>
        [Required]
        public AppIdentityConfiguration AppIdentity { get; set; }

        /// <summary>
        /// Gets/sets information about this Mona application's Marketplace AAD identity.
        /// </summary>
        [Required]
        public MarketplaceIdentityConfiguration MarketplaceIdentity { get; set; }
    }
}