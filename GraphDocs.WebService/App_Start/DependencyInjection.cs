using Autofac;
using Autofac.Integration.Mvc;
using GraphDocs.Core;
using GraphDocs.Core.Interfaces;
using GraphDocs.Infrastructure;
using GraphDocs.Infrastructure.Database;
using GraphDocs.Infrastructure.Workflow;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GraphDocs.WebService
{
    public class DependencyInjection
    {
        public static IContainer ConfigureDI()
        {
            var builder = new ContainerBuilder();

            // Register your MVC controllers.
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            //// Register classes in Autofac
            //builder.RegisterType<SettingsService>().As<ISettingsService>()
            //    .OnActivating(e =>
            //    {
            //        e.Instance.APIVersionNumber = "v1";
            //        e.Instance.SiteBaseUrl = GetCurrentDomain();
            //        e.Instance.SmtpServer = "localhost";
            //        e.Instance.WorkflowFolder = HttpContext.Current.Server.MapPath("~/WorkflowDefinitions");
            //        e.Instance.WorkflowStoreId = new Guid("c068fd97-117e-4bac-b93a-613d7baaa088");
            //    })
            //    .InstancePerRequest();

            builder.RegisterType<Neo4jConnectionFactory>().As<IConnectionFactory>();
            builder.RegisterType<WorkflowService>()
                .WithParameter("workflowFolder", HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["WorkflowFolder"]))
                .WithParameter("workflowStoreId", new Guid(ConfigurationManager.AppSettings["WorkflowStoreId"]))
                .As<WorkflowService>();

            builder.RegisterType<DocumentFilesDataService>().As<DocumentFilesDataService>();
            builder.RegisterType<DocumentsDataService>().As<DocumentsDataService>();
            builder.RegisterType<FoldersDataService>().As<FoldersDataService>();
            builder.RegisterType<PathsDataService>().As<PathsDataService>();
            builder.RegisterType<TagsDataService>().As<TagsDataService>();
            builder.RegisterType<WorkflowService>().As<WorkflowService>();
            builder.RegisterType<DocumentsWorkflowsService>().As<DocumentsWorkflowsService>();

            // Build DI container
            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            return container;
        }

        public static string GetCurrentDomain()
        {
            // From SO answer: http://stackoverflow.com/questions/61817/whats-the-best-method-in-asp-net-to-obtain-the-current-domain
            var requestedUrl = HttpContext.Current.Request.Url;
            return requestedUrl.Scheme + "://" + requestedUrl.Host + (requestedUrl.IsDefaultPort ? "" : ":" + requestedUrl.Port);
        }
    }
}