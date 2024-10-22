// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mona.SaaS.Core.Models.Web;
using System.Threading.Tasks;

namespace Mona.SaaS.Core.Interfaces
{
    public interface ISubscriptionWebService
    {
        Task<IActionResult> OnLanding(HttpContext httpContext, string subToken = null);
        Task<IActionResult> OnWebhookNotification(HttpContext httpContext, WebhookNotification whNotification);
    }
}
