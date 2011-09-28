using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.ServiceRuntime;
using WebRole.Models;
using System.Diagnostics;
using System.Threading;
using System.Text;

namespace WebRole.Controllers
{
    public class SuggestController : Controller
    {
        private static string randombase26()
        {
            var number = new Random().Next(456976); // 26^4 = 456976
            var sb = new StringBuilder(4);
            while (number > 0)
            {
                sb.Insert(0, (char)((int)'a' + number % 26));
                number /= 26;
            }
            return new string('a', 4 - sb.Length) + sb.ToString();
        }
        
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Submit(string text)
        {
            var account = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));
            string audioBlob = null;
            foreach (var format in new[] { "mp3", "ogg" })
            {
                if (Request.Files[format].ContentLength > 0)
                {
                    audioBlob = audioBlob ?? randombase26();
                    var blob = account.CreateCloudBlobClient().GetContainerReference("audio")
                        .GetBlobReference(audioBlob + "." + format);
                    blob.Properties.ContentType = "audio/" + format;
                    blob.Properties.CacheControl = "max-age=86400"; // 60 seconds * 60 minutes * 24 hours = 1 day
                    blob.UploadFromStream(Request.Files[format].InputStream);
                }
            }
            var ctx = account.CreateCloudTableClient().GetDataServiceContext();
            ctx.AddObject("compliments", new Compliment(text, audioBlob));
            ctx.SaveChangesWithRetries();
            return RedirectToAction("index");
        }
    }
}