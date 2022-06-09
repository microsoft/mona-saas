// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Mona.SaaS.Web.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}