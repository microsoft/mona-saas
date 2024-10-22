using System.ComponentModel.DataAnnotations;

namespace Mona.SaaS.Core.Models.Configuration
{
    public class ManagedIdentityConfiguration
    {
        /// <summary>
        /// Gets/sets the external managed identity (Marketplace auth) Azure AD client ID
        /// </summary>
        [Required]
        public string ExternalClientId { get; set; }

        /// <summary>
        /// Gets/sets the external managed identity (Marketplace auth) Azure resource ID
        /// </summary>
        [Required]
        public string ExternalManagedId { get; set; }

        /// <summary>
        /// Gets/sets the external managed identity (Marketplace auth) Azure AD service principal ID
        /// </summary>
        [Required]
        public string ExternalPrincipalId { get; set; }

        /// <summary>
        /// Gets/sets the internal managed identity (internal resource auth) Azure resource ID
        /// </summary>
        [Required]
        public string InternalManagedId { get; set; }

        /// <summary>
        /// Gets/sets the internal managed identity (internal resource auth) Azure AD service principal ID
        /// </summary>
        [Required]
        public string InternalPrincipalId { get; set; }
    }
}
