﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.Web.Models;

namespace Mona.SaaS.Admin.Web.Controllers
{
    public class SetupController : Controller
    {
        private readonly ILogger logger;
        private readonly DeploymentConfiguration deploymentConfig;
        private readonly IPublisherConfigurationStore publisherConfigStore;

        public SetupController(
            IOptionsSnapshot<DeploymentConfiguration> deploymentConfig,
            ILogger<SetupController> logger,
            IPublisherConfigurationStore publisherConfigStore)
        {
            this.deploymentConfig = deploymentConfig.Value;
            this.logger = logger;
            this.publisherConfigStore = publisherConfigStore;
        }

        [HttpGet, Route("setup", Name = "setup")]
        public async Task<IActionResult> Index()
        {
            // TODO: When/where can the setup screen be accessed?

            var publisherConfig = await publisherConfigStore.GetPublisherConfiguration();

            return View(new SetupModel(publisherConfig) { MonaVersion = deploymentConfig.MonaVersion });
        }

        [HttpPost, Route("setup", Name = "setup"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SetupModel setupModel)
        {
            if (ModelState.IsValid == false)
            {
                return View(setupModel);
            }

            setupModel.Publisher.IsSetupComplete = true;

            await publisherConfigStore.PutPublisherConfiguration(setupModel.Publisher);

            return RedirectToRoute("admin");
        }
    }
}