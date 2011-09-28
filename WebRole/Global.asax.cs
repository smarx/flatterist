using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebRole
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("api", "api/{action}/{id}", new { controller = "api", action = "index", id = UrlParameter.Optional });
            routes.MapRoute("suggest", "suggest/{action}", new { controller = "suggest", action = "index" });
            routes.MapRoute("admin", "admin/{action}/{id}", new { controller = "admin", action = "index", id = UrlParameter.Optional });
            routes.MapRoute("credits", "credits", new { controller = "home", action = "credits" });
            routes.MapRoute("reload", "reload", new { controller = "home", action = "reload" });
            routes.MapRoute("default", "{id}", new { controller = "home", action = "index", id = UrlParameter.Optional });
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}