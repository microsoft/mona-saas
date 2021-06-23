// MICROSOFT CONFIDENTIAL INFORMATION
//
// Copyright © Microsoft Corporation
//
// Microsoft Corporation (or based on where you live, one of its affiliates) licenses this preview code for your internal testing purposes only.
//
// Microsoft provides the following preview code AS IS without warranty of any kind. The preview code is not supported under any Microsoft standard support program or services.
//
// Microsoft further disclaims all implied warranties including, without limitation, any implied warranties of merchantability or of fitness for a particular purpose. The entire risk arising out of the use or performance of the preview code remains with you.
//
// In no event shall Microsoft be liable for any damages whatsoever (including, without limitation, damages for loss of business profits, business interruption, loss of business information, or other pecuniary loss) arising out of the use of or inability to use the preview code, even if Microsoft has been advised of the possibility of such damages.

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