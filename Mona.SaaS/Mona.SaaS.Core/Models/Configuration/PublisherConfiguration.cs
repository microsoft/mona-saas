// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Constants;
using System.ComponentModel.DataAnnotations;

namespace Mona.SaaS.Core.Models.Configuration
{
    public class PublisherConfiguration
    {
        /// <summary>
        /// Indicates whether or not the setup wizard has been copleted.
        /// </summary>
        public bool IsSetupComplete { get; set; }

        /// <summary>
        /// Gets/sets the publisher's friendly/display name.
        /// </summary>
        [Required, Display(Name = "Publisher name")]
        public string PublisherDisplayName { get; set; }

        /// <summary>
        /// Gets/sets the URL of the publisher's home page.
        /// </summary>
        [Display(Name = "Publisher home page URL")]
        public string PublisherHomePageUrl { get; set; }

        /// <summary>
        /// Gets/sets the URL of the publisher's privacy notice page.
        /// </summary>
        [Display(Name = "Publisher privacy notice URL")]
        public string PublisherPrivacyNoticePageUrl { get; set; }

        /// <summary>
        /// Gets/sets the URL of the publisher's contact page.
        /// </summary>
        [Display(Name = "Publisher contact page URL")]
        public string PublisherContactPageUrl { get; set; }

        /// <summary>
        /// Gets/sets the publisher's default copyright notice.
        /// </summary>
        [Display(Name = "Publisher copyright notice")]
        public string PublisherCopyrightNotice { get; set; }

        /// <summary>
        /// Gets/sets the URL of the page that users should be redirected to to configure an existing subscription.
        /// </summary>
        /// <remarks>
        /// Note that you can use [<see cref="ConfigurationFields.SubscriptionId"/>] 
        /// here to automatically merge the applicable subscription ID (e.g., <c>"https://microsoft.com/{subscription-id}/mona/..."</c>).
        /// </remarks>
        [Required, Display(Name = "SaaS offer configuration URL")]
        public string SubscriptionConfigurationUrl { get; set; }

        /// <summary>
        /// Gets/sets the URL of the page that users should be redirected to once their subscription purchase is complete.
        /// </summary>
        /// <remarks>
        /// Note that you can use [<see cref="ConfigurationFields.SubscriptionId"/>] 
        /// here to automatically merge the applicable subscription ID (e.g., <c>"https://microsoft.com/{subscription-id}/mona/..."</c>).
        /// </remarks>
        [Required, Display(Name = "SaaS offer purchase confirmation URL")]
        public string SubscriptionPurchaseConfirmationUrl { get; set; }
    }
}