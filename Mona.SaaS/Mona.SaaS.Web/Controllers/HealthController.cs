// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Web.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Mona.SaaS.Core.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    public class HealthController : Controller
    {
        private readonly IMarketplaceOperationService mpOperationService;
        private readonly IMarketplaceSubscriptionService mpSubscriptionService;
        private readonly IPublisherConfigurationStore publisherConfigStore;
        private readonly ISubscriptionEventPublisher subEventPublisher;
        private readonly ISubscriptionStagingCache subStagingCache;
        private readonly ISubscriptionTestingCache subTestingCache;
        private readonly ILogger logger;

        public HealthController(
            IMarketplaceOperationService mpOperationService,
            IMarketplaceSubscriptionService mpSubscriptionService,
            IPublisherConfigurationStore publisherConfigStore,
            ISubscriptionEventPublisher subEventPublisher,
            ISubscriptionStagingCache subStagingCache,
            ISubscriptionTestingCache subTestingCache,
            ILogger<HealthController> logger)
        {
            this.mpOperationService = mpOperationService;
            this.mpSubscriptionService = mpSubscriptionService;
            this.publisherConfigStore = publisherConfigStore;
            this.subEventPublisher = subEventPublisher;
            this.subStagingCache = subStagingCache;
            this.subTestingCache = subTestingCache;
            this.logger = logger;
        }

        [AllowAnonymous, HttpGet, Route("health", Name = "health")]
        public IActionResult CheckHealth()
        {
            // Health check is unauthenticated which is always kind of scary but this endpoint _only_ 
            // returns a status code and doesn't really reveal anything about the app. Tools that check health status 
            // (e.g., K8s, Front Door, AppGw, etc.) don't allow you to authenticate to health check endpoints anyway
            // so let's just keep things simple and reasonably secure.

            try
            {
                var checkTasks = new List<Task<bool>>
                {
                    mpOperationService.IsHealthyAsync(),    // Check connectivity to Marketplace Operations API
                    mpSubscriptionService.IsHealthyAsync(), // Check connectivity to Marketplace Subscriptions API
                    publisherConfigStore.IsHealthyAsync(),  // Check connectivity to publisher configuration store (blob storage by default)
                    subEventPublisher.IsHealthyAsync(),     // Check connectivity to subscription events topic (event grid by default)
                    subStagingCache.IsHealthyAsync(),       // Check connectivity to subscription staging cache (blob storage by default)
                    subTestingCache.IsHealthyAsync()        // Check connectivity to subscription testing cache (blob storage by default)
                };   

                Task.WaitAll(checkTasks.ToArray()); // Wait for all the checks to finish.

                // Everything's good    == OK (200)
                // Something's broken   == Service Unavailable (503)

                return StatusCode(checkTasks.All(t => t.Result) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.ServiceUnavailable);

                // If something's broken, we should be able to see it in the logs. We don't return details here since
                // this is an unauthenticated endpoint and we really don't want to reveal anything more than we absolutely need to.
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "An error occurred while checking the health of this Mona deployment. " +
                    "See exception for details.");

                // If something broke during the health check, we're definitely not healthy (503)...

                return StatusCode((int)HttpStatusCode.ServiceUnavailable);
            }
        }
    }
}
