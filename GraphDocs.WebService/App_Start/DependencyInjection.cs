using Autofac;
using GraphDocs.Core;
using GraphDocs.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GraphDocs.WebService
{
    public class DependencyInjection
    {
        public static IContainer ConfigureDI()
        {
            var builder = new ContainerBuilder();

            // Register classes in Autofac
            builder.RegisterType<SettingsService>().As<ISettingsService>()
                .OnActivating(e =>
                {
                    e.Instance.APIVersionNumber = "v1";
                    e.Instance.SiteBaseUrl = GetCurrentDomain();
                    e.Instance.SmtpServer = "localhost";
                })
                .InstancePerRequest();
            
            // Build DI container
            var container = builder.Build();
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