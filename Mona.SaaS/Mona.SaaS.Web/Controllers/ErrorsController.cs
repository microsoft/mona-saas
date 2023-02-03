using Microsoft.AspNetCore.Mvc;
using System;

namespace Mona.SaaS.Web.Controllers
{
    public class ErrorsController : Controller
    {
        //controller for the Error access denied view
        [HttpGet, Route("Errors/ErrorAccessDenied", Name = "Error")]
        public IActionResult ErrorAccess()
        {
            ViewData["ErrorDescription"] = "Mona cannot be accessed using a guest or personal account.  Please sign out and use a Workplace or School acount.";
            ViewData["ErrorTitle"] = "Error Access Denied";
            return View();
        }
    }
}
