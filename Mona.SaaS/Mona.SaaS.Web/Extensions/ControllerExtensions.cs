// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Mona.SaaS.Web.Extensions
{
    public static class ControllerExtensions
    {
        public static string TryToGetCurrentUserGivenName(this Controller controller, string defaultName = null)
        {
            return defaultName;
        }
    }
}