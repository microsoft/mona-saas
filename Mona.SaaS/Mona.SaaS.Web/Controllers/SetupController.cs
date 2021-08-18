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