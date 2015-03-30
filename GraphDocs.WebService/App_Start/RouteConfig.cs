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
                name: "Folders_Route",
                url: "v1/Folders/{*path}",
                defaults: new { action = "Index", controller = "Folders", path = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Documents_Route",
                url: "v1/Documents/{*path}",
                defaults: new { action = "Index", controller = "Documents", path = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "WorkflowDefinitions_Route",
                url: "v1/WorkflowDefinitions/{folderId}/{workflowName}",
                defaults: new { action = "Index", controller = "WorkflowDefinitions" }
            );
            routes.MapRoute(
                name: "Default_Route",
                url: "v1/{controller}/{action}/{id}",
                defaults: new { action = "Index", path = UrlParameter.Optional, id = UrlParameter.Optional }
            );
        }
    }
}
