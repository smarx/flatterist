using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Caching;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string id)
        {
            ViewBag.BackgroundBaseUrl = RoleEnvironment.GetConfigurationSettingValue("BackgroundBaseUrl");
            return View((object)id);
        }

        public ActionResult Credits()
        {
            return View();
        }
    }
}