using System;
using Owin;
using Microsoft.Owin;
using Microsoft.AspNet.SignalR;

[assembly: OwinStartup(typeof(ErlangVMA.Startup))]
namespace ErlangVMA
{
	public class Startup
	{
		public Startup()
		{
		}

		public void Configuration(IAppBuilder appBuilder)
		{
			appBuilder.MapSignalR("/signalr", new HubConfiguration()
			{
				EnableJSONP = false,
				EnableDetailedErrors = true,
				EnableJavaScriptProxies = true,
			});
		}
	}
}
