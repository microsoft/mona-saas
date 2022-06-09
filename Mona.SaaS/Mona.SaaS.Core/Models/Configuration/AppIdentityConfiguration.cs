// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;

namespace Mona.SaaS.Core.Models.Configuration
{
    /// <summary>
    /// This Mona application's Azure Active Directory (AAD) configuration information.
    /// </summary>
    public class AppIdentityConfiguration
    {
        /// <summary>
        /// Gets/sets this Mona application's AAD client ID.
        /// </summary>
        [Required]
        public string AadClientId { get; set; }

        /// <summary>
        /// Gets/sets this Mona application's AAD client secret.
        /// </summary>
        [Required]
        public string AadClientSecret { get; set; }

        /// <summary>
        /// Gets/sets this Mona application's AAD tenant ID.
        /// </summary>
        [Required]
        public string AadTenantId { get; set; }

        /// <summary>
        /// Gets/sets this Mona application's AAD enterprise application/service principal object ID.
        /// </summary>
        [Required]
        public string AadPrincipalId { get; set; }
    }
}