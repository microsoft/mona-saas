// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models.Web;

namespace Mona.SaaS.Subscriber.Web.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionWebService subscriptionService;

        public SubscriptionController(ISubscriptionWebService subscriptionService) =>
            this.subscriptionService = subscriptionService;

        [AllowAnonymous, HttpGet, Route("/", Name = "landing")]
        public Task<IActionResult> OnLanding(string? subToken = null) =>
            subscriptionService.OnLanding(HttpContext, subToken);

        [AllowAnonymous, HttpPost, Route("/webhook", Name = "webhook")]
        private Task<IActionResult> OnWehbookNotification([FromBody] WebhookNotification whNotification) =>
            subscriptionService.OnWebhookNotification(HttpContext, whNotification);
    }
}

