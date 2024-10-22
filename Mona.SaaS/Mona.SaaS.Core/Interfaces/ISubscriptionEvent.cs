// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Interfaces
{
    using System;

    public interface ISubscriptionEvent
    {
         string EventId { get; set; }
         string EventType { get; set; }
         string EventVersion { get; set; }
         string OperationId { get; set; }
         string SubscriptionId { get; set; }

         DateTime OperationDateTimeUtc { get; set; }
    }
}
