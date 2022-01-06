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

namespace Mona.SaaS.Services.Default.Models
{
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// Represents a subscription operation as defined by the Marketplace API.
    /// </summary>
    /// <remarks>
    /// Since the .NET Marketplace SDK doesn't yet support the renew operation (as of 1/6/2022), we're using this model instead to directly
    /// call the Marketplace API operation endpoints inside [<see cref="DefaultMarketplaceClient"/>]. More information on this model can be found at 
    /// [https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/pc-saas-fulfillment-operations-api].
    /// </remarks>
    public class Operation
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("activityId")]
        public string ActivityId { get; set; }

        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("offerId")]
        public string OfferId { get; set; }

        [JsonProperty("publisherId")]
        public string PublisherId { get; set; }

        [JsonProperty("planId")]
        public string PlanId { get; set; }

        [JsonProperty("quantity")]
        public int? Quantity { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("timeStamp")]
        public DateTime? TimeStamp { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("errorStatusCode")]
        public string ErrorStatusCode { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
