// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;

namespace Mona.SaaS.Core.Models.Web
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
            public const string Renew = "Renew";
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
