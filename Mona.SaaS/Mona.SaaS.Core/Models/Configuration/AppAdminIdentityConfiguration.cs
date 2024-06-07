// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;

namespace Mona.SaaS.Core.Models.Configuration
{
    /// <summary>
    /// Provides information on this Mona application's Azure Active Directory (AAD) administrative identity.
    /// </summary>
    /// <remarks>
    /// Typically, this identity will be the same as the user that originally deployed this Mona application.
    /// </remarks>
    public class AppAdminIdentityConfiguration
    {
        /// <summary>
        /// Gets/sets this Mona application's administrative AAD tenant ID.
        /// </summary>
        /// <remarks>
        /// AAD claim is [tid].
        /// </remarks>
        [Required]
        public string AadTenantId { get; set; }
    }
}