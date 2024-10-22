﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Constants;
using System;

namespace Mona.SaaS.Core.Models.Events.V_2021_05_01
{
    /// <summary>
    /// Event generated by Azure Marketplace when a new subscription is purchased.
    /// </summary>
    public class SubscriptionPurchased : BaseSubscriptionEvent
    {
        public SubscriptionPurchased() : base(EventTypes.SubscriptionPurchased) { }

        public SubscriptionPurchased(Subscription subscription, string operationId = null, DateTime? operationDateTimeUtc = null)
            : base(EventTypes.SubscriptionPurchased, subscription, operationId, operationDateTimeUtc) { }
    }
}