// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;

namespace Mona.SaaS.Core.Models.Configuration
{
    /// <summary>
    /// Mona admin app identity configuration
    /// </summary>
    public class AdminAppIdentityConfiguration
    {
        /// <summary>
        /// Gets/sets the admin app's Entra client ID
        /// </summary>
        [Required]
        public string EntraAppId { get; set; }

        /// <summary>
        /// Gets/sets the admin app's Entra tenant ID
        /// </summary>
        [Required]
        public string EntraTenantId { get; set; }
    }
}