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

using Newtonsoft.Json;
using System;

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
        public string SubscriptionId { get; set; }

        [JsonProperty("Subscription Name")]
        public string SubscriptionName { get; set; }

        [JsonProperty("Offer ID")]
        public string OfferId { get; set; }

        [JsonProperty("Plan ID")]
        public string PlanId { get; set; }

        [JsonProperty("Is Test Subscription?")]
        public bool IsTest { get; set; }

        [JsonProperty("Is Free Trial Subscription?")]
        public bool IsFreeTrial { get; set; }

        [JsonProperty("Seat Quantity")]
        public int? SeatQuantity { get; set; }

        [JsonProperty("Subscription Status")]
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