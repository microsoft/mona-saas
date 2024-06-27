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
        /// Gets/sets information about Mona's admin app Entra identity
        /// </summary>
        [Required]
        public AdminAppIdentityConfiguration AdminAppIdentity { get; set; }

        /// <summary>
        /// Gets/sets information about Mona's Azure managed identities
        /// </summary>
        [Required]
        public ManagedIdentityConfiguration ManagedIdentities { get; set; }
    }
}