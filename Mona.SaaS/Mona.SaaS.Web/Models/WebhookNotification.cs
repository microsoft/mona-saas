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

namespace Mona.SaaS.Web.Models
{
    public class WebhookNotification
    {
        public static class ActionTypes
        {
            public const string ChangePlan = "ChangePlan";
            public const string ChangeQuantity = "ChangeQuantity";
            public const string Suspend = "Suspend";
            public const string Unsubscribe = "Unsubscribe";
            public const string Reinstate = "Reinstate";
        }

        public static class Statuses
        {
            public const string InProgress = "InProgress";
            public const string Success = "Success";
        }

        [JsonProperty("id")]
        public string OperationId { get; set; }

        [JsonProperty("activityId")]
        public string ActivityId { get; set; }

        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("publisherId")]
        public string PublisherId { get; set; }

        [JsonProperty("offerId")]
        public string OfferId { get; set; }

        [JsonProperty("planId")]
        public string PlanId { get; set; }

        [JsonProperty("quantity")]
        public int? SeatQuantity { get; set; }

        [JsonProperty("timeStamp")]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty("action")]
        public string ActionType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}