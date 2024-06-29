// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models.Web;

namespace Mona.SaaS.Admin.Web.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionWebService subscriptionService;

        public SubscriptionController(ISubscriptionWebService subscriptionService) =>
            this.subscriptionService = subscriptionService;

        [Authorize, HttpGet, Route("/test", Name = "landing/test")]
        public Task<IActionResult> OnLanding() =>
            subscriptionService.OnLanding(HttpContext);

        [AllowAnonymous, HttpPost, Route("/webhook/test", Name = "webhook/test")]
        public Task<IActionResult> ProcessTestWebhookNotificationAsync([FromBody] WebhookNotification whNotification) =>
            subscriptionService.OnWebhookNotification(HttpContext, whNotification);
    }
}

