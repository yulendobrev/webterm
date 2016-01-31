using System;
using Owin;
using Microsoft.Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Security.Cookies;

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
            appBuilder.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ApplicationCookie",
                LoginPath = new PathString("/Authentication/Login")
            });

            appBuilder.MapSignalR("/signalr", new HubConfiguration()
            {
                EnableJSONP = false,
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = true,
                Resolver = new SignalRDependencyResolver(DependencyManager.Instance)
            });
        }
    }
}
