using Mona.SaaS.Core.Models.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Mona.SaaS.Admin.Web.Models.Admin
{
    public class UserRedirectionSettings
    {
        public UserRedirectionSettings() { }

        public UserRedirectionSettings(PublisherConfiguration publisherConfig)
        {
            ArgumentNullException.ThrowIfNull(publisherConfig, nameof(publisherConfig));

            SaaSHomePageUrl = publisherConfig.PublisherHomePageUrl;
            SubscriptionConfigurationUrl = publisherConfig.SubscriptionConfigurationUrl;
            SubscriptionLandingUrl = publisherConfig.SubscriptionPurchaseConfirmationUrl;
        }

        [Required, Display(Name = "SaaS home page URL")]
        public string? SaaSHomePageUrl { get; set; }

        [Required, Display(Name = "Subscription configuration URL")]
        public string? SubscriptionConfigurationUrl { get; set; }

        [Required, Display(Name = "Subscription purchased URL")]
        public string? SubscriptionLandingUrl { get; set; }
    }
}
