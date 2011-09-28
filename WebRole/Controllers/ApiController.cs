using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using WebRole.Models;
using System.Web.UI;
using System.Data.Services.Client;

namespace WebRole.Controllers
{
    public class ApiController : Controller
    {
        private JsonResult JsonForCompliment(Compliment compliment)
        {
            string url = null;
            if (!string.IsNullOrEmpty(compliment.AudioBlob))
            {
                var ub = new UriBuilder(CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"))
                    .CreateCloudBlobClient().GetContainerReference("audio").GetBlobReference(compliment.AudioBlob).Uri);
                ub.Scheme = "http";
                if (ub.Port == 443) ub.Port = 80; // Don't touch it if it's 10000 for local storage
                var customDomain = RoleEnvironment.GetConfigurationSettingValue("CustomDomain");
                if (!string.IsNullOrEmpty(customDomain)) ub.Host = customDomain;
                url = ub.Uri.AbsoluteUri;
            }
            var result = Json(new
            {
                Text = compliment.Text,
                AudioBaseUrl = url,
                Id = compliment.RowKey
            });
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return result;
        }

        public JsonResult GetCompliment(string id)
        {
            var account = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));
            var ctx = account.CreateCloudTableClient().GetDataServiceContext();
            return JsonForCompliment(ctx.CreateQuery<Compliment>("compliments").Where(c => c.PartitionKey == string.Empty && c.RowKey == id).Single());
        }

        [OutputCache(Duration = 0, NoStore = true, VaryByParam = "*", Location = OutputCacheLocation.None)]
        public JsonResult GetRandomCompliment()
        {
            var account = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));
            var ctx = account.CreateCloudTableClient().GetDataServiceContext();
            try
            {
                var compliments = (from c in ctx.CreateQuery<Compliment>("compliments")
                                   where c.PartitionKey == string.Empty && c.Approved
                                   select c).AsTableServiceQuery().ToList();

                var compliment = compliments[new Random().Next(compliments.Count)];
                return JsonForCompliment(compliment);
            }
            catch
            {
                return JsonForCompliment(new Compliment("Sorry, no compliments have been added yet.", ""));
            }            
        }
    }
}