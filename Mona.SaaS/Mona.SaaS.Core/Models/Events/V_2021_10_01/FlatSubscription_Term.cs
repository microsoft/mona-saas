// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;

namespace Mona.SaaS.Core.Models.Events
{
    public partial class FlatSubscription
    {
        [JsonProperty("Subscription Term Unit")]
        public string TermUnit { get; set; }

        [JsonProperty("Subscription Start Date")]
        public DateTime? SubscriptionStartDate { get; set; }

        [JsonProperty("Subscription End Date")]
        public DateTime? SubscriptionEndDate { get; set; }

        private void ApplyTermInfo(MarketplaceTerm term)
        {
            if (term != null)
            {
                TermUnit = term.TermUnit;
                SubscriptionStartDate = term.StartDate;
                SubscriptionEndDate = term.EndDate;
            }
        }
    }
}
