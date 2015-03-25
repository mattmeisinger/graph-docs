using Autofac;
using Autofac.Integration.Mvc;
using GraphDocs.Core;
using GraphDocs.Core.Interfaces;
using GraphDocs.Infrastructure.Workflow;
using System;
using System.Collections.Generic;
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

            // Register classes in Autofac
            builder.RegisterType<SettingsService>().As<ISettingsService>()
                .OnActivating(e =>
                {
                    e.Instance.APIVersionNumber = "v1";
                    e.Instance.SiteBaseUrl = GetCurrentDomain();
                    e.Instance.SmtpServer = "localhost";
                    e.Instance.WorkflowFolder = HttpContext.Current.Server.MapPath("~/WorkflowDefinitions");
                    e.Instance.WorkflowStoreId = new Guid("c068fd97-117e-4bac-b93a-613d7baaa088");
                })
                .InstancePerRequest();

            builder.RegisterType<WorkflowService>().As<IWorkflowService>();

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