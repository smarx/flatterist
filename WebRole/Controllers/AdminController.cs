using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class RequireBasicAuthentication : ActionFilterAttribute
    {
        private bool validate(string username, string password)
        {
            return username == RoleEnvironment.GetConfigurationSettingValue("AdminUsername")
                && password == RoleEnvironment.GetConfigurationSettingValue("AdminPassword");
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var req = filterContext.HttpContext.Request;
            if (!string.IsNullOrEmpty(req.Headers["Authorization"]))
            {
                var cred = System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(req.Headers["Authorization"].Substring(6))).Split(':');
                var user = new { Name = cred[0], Pass = cred[1] };
                if (validate(user.Name, user.Pass))
                {
                    filterContext.HttpContext.User = new GenericPrincipal(new GenericIdentity(user.Name), null);
                    return;
                }
            }
            var res = filterContext.HttpContext.Response;
            res.StatusCode = 401;
            res.AddHeader("WWW-Authenticate", "Basic realm=\"" + req.Headers["Host"] + "\"");
            filterContext.Result = new HttpUnauthorizedResult();
        }
    }

    public class AdminController : Controller
    {
        [RequireBasicAuthentication]
        public ActionResult Index()
        {
            var account = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));
            var compliments = account.CreateCloudTableClient().GetDataServiceContext().CreateQuery<Compliment>("compliments").Where(c => c.PartitionKey == string.Empty).AsTableServiceQuery().ToList();

            return View(compliments);
        }

        [RequireBasicAuthentication, HttpPost]
        public ActionResult Approve(string id, bool approved)
        {
            var ctx = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString")).CreateCloudTableClient().GetDataServiceContext();
            var compliment = ctx.CreateQuery<Compliment>("compliments").Where(c => c.PartitionKey == string.Empty && c.RowKey == id).Single();
            compliment.Approved = approved;
            ctx.UpdateObject(compliment);
            ctx.SaveChangesWithRetries();
            return new EmptyResult();
        }
    }
}