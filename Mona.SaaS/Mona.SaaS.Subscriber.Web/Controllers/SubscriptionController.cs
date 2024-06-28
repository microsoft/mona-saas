// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models.Web;
using System.Net;

namespace Mona.SaaS.Subscriber.Web.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ILogger log;
        private readonly ISubscriptionWebService subscriptionService;

        public SubscriptionController(
            ILogger<SubscriptionController> log,
            ISubscriptionWebService subscriptionService)
        {
            this.log = log;
            this.subscriptionService = subscriptionService;
        }

        [AllowAnonymous, HttpGet, Route("/", Name = "landing")]
        public async Task<IActionResult> OnLanding(string? subToken = null)
        {
            try
            {
                return await subscriptionService.OnLanding(HttpContext, subToken);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while handling a live landing. See exception for details");

                return new NotFoundResult(); // Nothing to see here...
            }
        }

        [AllowAnonymous, HttpPost, Route("/webhook", Name = "webhook")]
        private async Task<IActionResult> OnWehbookNotification([FromBody] WebhookNotification whNotification)
        {
            try
            {
                return await subscriptionService.OnWebhookNotification(HttpContext, whNotification);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while handling a live webhook notification. See exception for details.");

                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable); // Hopefully it's temporary...
            }
        }
    }
}

