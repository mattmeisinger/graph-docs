using GraphDocs.DataServices;
using System;
using System.ServiceModel.Activation;
using System.Web.Routing;

namespace GraphDocs.WebService
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            // This route allows the web service to be called using /folders instead of the /FolderService.svc relative URL.
            RouteTable.Routes.Add(new ServiceRoute("folders", new WebServiceHostFactory(), typeof(FoldersService)));

            // Initialize Neo4J database
            DatabaseService.Init();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}