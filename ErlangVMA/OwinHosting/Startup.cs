using System;
using Owin;
using Microsoft.Owin;

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
			appBuilder.MapHubs("signalr", new Microsoft.AspNet.SignalR.HubConfiguration()
			{
				EnableCrossDomain = true,
				EnableDetailedErrors = true,
				EnableJavaScriptProxies = true,
			});
		}
	}
}

