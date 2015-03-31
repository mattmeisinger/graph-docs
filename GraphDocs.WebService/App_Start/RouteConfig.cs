using System;
using System.Collections.Generic;
using System.Configuration;
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

            var apiVersion = ConfigurationManager.AppSettings["APIVersionNumber"];
            routes.MapRoute(
                name: "Home_Route",
                url: "",
                defaults: new { action = "Index", controller = "Home" }
            );
            routes.MapRoute(
                name: "Folders_Route",
                url: apiVersion + "/Folders/{*path}",
                defaults: new { action = "Index", controller = "Folders", path = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Documents_Route",
                url: apiVersion + "/Documents/{*path}",
                defaults: new { action = "Index", controller = "Documents" }
            );
            routes.MapRoute(
                name: "DocumentFiles_Route",
                url: apiVersion + "/DocumentFiles/{*path}",
                defaults: new { action = "Index", controller = "DocumentFiles" }
            );
            routes.MapRoute(
                name: "WorkflowDefinitions_Route",
                url: apiVersion + "/WorkflowDefinitions/{folderId}/{workflowName}",
                defaults: new { action = "Index", controller = "WorkflowDefinitions" }
            );
            routes.MapRoute(
                name: "Default_Route",
                url: apiVersion + "/{controller}/{action}/{id}",
                defaults: new { action = "Index", path = UrlParameter.Optional, id = UrlParameter.Optional }
            );
        }
    }
}
