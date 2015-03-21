using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GraphDocs.WebService
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "RestApiV1",
                url: "v1/{controller}/{*path}",
                defaults: new { action = "Index", path = UrlParameter.Optional }
            );
        }
    }
}
