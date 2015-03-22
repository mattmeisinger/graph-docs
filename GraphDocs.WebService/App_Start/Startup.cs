using Autofac;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GraphDocs.WebService
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Autofac middleware must be registered before other components, as the other components will probably need to use it.
            app.UseAutofacMiddleware(DependencyInjection.ConfigureDI());

        }
    }
}