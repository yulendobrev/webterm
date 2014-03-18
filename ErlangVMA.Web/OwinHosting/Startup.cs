using System;
using Owin;
using Microsoft.Owin;
using Microsoft.AspNet.SignalR;

[assembly: OwinStartup(typeof(ErlangVMA.Web.Startup))]

namespace ErlangVMA.Web
{
    public class Startup
    {
        public Startup()
        {
        }

        public void Configuration(IAppBuilder appBuilder)
        {
            var dependencyManager = new DependencyManager();

            appBuilder.MapSignalR("/signalr", new HubConfiguration()
            {
                EnableJSONP = false,
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = true,
                Resolver = new SignalRDependencyResolver(dependencyManager)
            });
        }
    }
}
