// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Mona.SaaS.Core.Models.Events
{
    /// <summary>
    /// Represents a "flattened" Azure Marketplace SaaS subscription suitable for downstream consumption by Power Automate/Logic Apps.
    /// </summary>
    public partial class FlatSubscription
    {
        public FlatSubscription() { }

        public FlatSubscription(Subscription sourceSubscription)
        {
            if (sourceSubscription == null)
            {
                throw new ArgumentNullException(nameof(sourceSubscription));
            }

            ApplyBasicInfo(sourceSubscription);
            ApplyTermInfo(sourceSubscription.Term);
            ApplyBeneficiaryInfo(sourceSubscription.Beneficiary);
            ApplyPurchaserInfo(sourceSubscription.Purchaser);
        }

        [JsonProperty("Subscription ID")]
        [JsonPropertyName("Subscription ID")]
        public string SubscriptionId { get; set; }

        [JsonProperty("Subscription Name")]
        [JsonPropertyName("Subscription Name")]
        public string SubscriptionName { get; set; }

        [JsonProperty("Offer ID")]
        [JsonPropertyName("Offer ID")]
        public string OfferId { get; set; }

        [JsonProperty("Plan ID")]
        [JsonPropertyName("Plan ID")]
        public string PlanId { get; set; }

        [JsonProperty("Is Test Subscription?")]
        [JsonPropertyName("Is Test Subscription?")]
        public bool IsTest { get; set; }

        [JsonProperty("Is Free Trial Subscription?")]
        [JsonPropertyName("Is Free Trial Subscription?")]
        public bool IsFreeTrial { get; set; }

        [JsonProperty("Seat Quantity", NullValueHandling = NullValueHandling.Include)]
        [JsonPropertyName("Seat Quantity")]
        public int? SeatQuantity { get; set; }

        [JsonProperty("Subscription Status")]
        [JsonPropertyName("Subscription Status")]
        public string Status { get; set; }

        private void ApplyBasicInfo(Subscription subscription)
        {
            SubscriptionId = subscription.SubscriptionId;
            SubscriptionName = subscription.SubscriptionName;
            OfferId = subscription.OfferId;
            PlanId = subscription.PlanId;
            IsTest = subscription.IsTest;
            IsFreeTrial = subscription.IsFreeTrial;
            SeatQuantity = subscription.SeatQuantity;
            Status = subscription.Status.ToString();
        }
    }
}