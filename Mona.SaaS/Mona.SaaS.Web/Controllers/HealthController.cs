// MICROSOFT CONFIDENTIAL INFORMATION
//
// Copyright © Microsoft Corporation
//
// Microsoft Corporation (or based on where you live, one of its affiliates) licenses this preview code for your internal testing purposes only.
//
// Microsoft provides the following preview code AS IS without warranty of any kind. The preview code is not supported under any Microsoft standard support program or services.
//
// Microsoft further disclaims all implied warranties including, without limitation, any implied warranties of merchantability or of fitness for a particular purpose. The entire risk arising out of the use or performance of the preview code remains with you.
//
// In no event shall Microsoft be liable for any damages whatsoever (including, without limitation, damages for loss of business profits, business interruption, loss of business information, or other pecuniary loss) arising out of the use of or inability to use the preview code, even if Microsoft has been advised of the possibility of such damages.

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
            try
            {
                var checkTasks = new List<Task<bool>>();

                checkTasks.Add(mpOperationService.IsHealthyAsync());
                checkTasks.Add(mpSubscriptionService.IsHealthyAsync());
                checkTasks.Add(publisherConfigStore.IsHealthyAsync());
                checkTasks.Add(subEventPublisher.IsHealthyAsync());
                checkTasks.Add(subStagingCache.IsHealthyAsync());
                checkTasks.Add(subTestingCache.IsHealthyAsync());

                Task.WaitAll(checkTasks.ToArray());

                return StatusCode(checkTasks.All(t => t.Result) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "An error occurred while checking the health of this Mona deployment. " +
                    "See exception for details.");

                return StatusCode((int)HttpStatusCode.ServiceUnavailable);
            }
        }
    }
}
