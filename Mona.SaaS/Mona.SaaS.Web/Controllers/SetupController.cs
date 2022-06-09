// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Web.Models;
using System.Threading.Tasks;

namespace Mona.SaaS.Web.Controllers
{
    [Authorize(Policy = "admin")]
    public class SetupController : Controller
    {
        private readonly ILogger logger;
        private readonly IPublisherConfigurationStore publisherConfigStore;

        public SetupController(
            ILogger<SetupController> logger,
            IPublisherConfigurationStore publisherConfigStore)
        {
            this.logger = logger;
            this.publisherConfigStore = publisherConfigStore;
        }

        [HttpGet, Route("setup", Name = "setup")]
        public async Task<IActionResult> Index()
        {
            // TODO: When/where can the setup screen be accessed?

            var publisherConfig = await this.publisherConfigStore.GetPublisherConfiguration();

            return View(new SetupModel(publisherConfig));
        }

        [HttpPost, Route("setup", Name = "setup"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SetupModel setupModel)
        {
            if (ModelState.IsValid == false)
            {
                return View(setupModel);
            }

            await this.publisherConfigStore.PutPublisherConfiguration(setupModel.Publisher);

            return RedirectToRoute("admin");
        }
    }
}