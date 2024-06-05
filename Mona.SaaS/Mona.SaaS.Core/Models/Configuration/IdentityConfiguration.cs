// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;

namespace Mona.SaaS.Core.Models.Configuration
{
    /// <summary>
    /// Provides Azure Active Directory (Azure AD) identity information about this Mona application
    /// </summary>
    public class IdentityConfiguration
    {
        /// <summary>
        /// Gets/sets information about this Mona deployment's administrative Azure AD identity
        /// </summary>
        [Required]
        public AppAdminIdentityConfiguration AdminIdentity { get; set; }

        /// <summary>
        /// Gets/sets information about this Mona deployment's Azure AD identity
        /// </summary>
        [Required]
        public AppIdentityConfiguration AppIdentity { get; set; }

        /// <summary>
        /// Gets/sets information about this Mona deployment's Azure managed identities
        /// </summary>
        [Required]
        public ManagedIdentityConfiguration ManagedIdentities { get; set; }
    }
}