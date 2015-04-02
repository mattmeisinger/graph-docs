using Autofac;
using Autofac.Integration.Mvc;
using GraphDocs.Core.Interfaces;
using GraphDocs.Infrastructure;
using GraphDocs.Infrastructure.Database;
using System;
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
                .As<IWorkflowService>();

            builder.RegisterType<DocumentFilesDataService>().As<IDocumentFilesDataService>();
            builder.RegisterType<DocumentsDataService>().As<IDocumentsDataService>();
            builder.RegisterType<FoldersDataService>().As<IFoldersDataService>();
            builder.RegisterType<PathsDataService>().As<IPathsDataService>();
            builder.RegisterType<TagsDataService>().As<ITagsDataService>();
            builder.RegisterType<DocumentsWorkflowsService>().As<IDocumentsWorkflowsService>();

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