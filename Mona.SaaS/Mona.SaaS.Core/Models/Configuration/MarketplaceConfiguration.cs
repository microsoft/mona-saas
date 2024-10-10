namespace Mona.SaaS.Core.Models.Configuration
{
    /// <summary>
    /// Information about Mona's marketplace landing page and webhook endpoints
    /// </summary>
    public class MarketplaceConfiguration
    {
        /// <summary>
        /// Gets/sets Mona's live marketplace landing page URL
        /// </summary>
        public string LandingPageUrl { get; set; }

        /// <summary>
        /// Gets/sets Mona's live marketplace webhook URL
        /// </summary>
        public string WebhookUrl { get; set; }
    }
}
