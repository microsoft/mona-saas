﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Constants;
using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Mona.SaaS.Core.Models.Events.V_2021_05_01
{
    /// <summary>
    /// Event generated by Azure Marketplace when a subscription plan is changed.
    /// </summary>
    public class SubscriptionPlanChanged : BaseSubscriptionEvent
    {
        public SubscriptionPlanChanged() : base(EventTypes.SubscriptionPlanChanged) { }

        public SubscriptionPlanChanged(Subscription subscription, string operationId, string newPlanId, DateTime? operationDateTimeUtc = null)
            : base(EventTypes.SubscriptionPlanChanged, subscription, operationId, operationDateTimeUtc)
        {
            NewPlanId = newPlanId;
        }

        [JsonProperty("newPlanId")]
        [JsonPropertyName("newPlanId")]
        public string NewPlanId { get; set; }
    }
}