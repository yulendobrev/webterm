using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Owin.Host.SystemWeb;

namespace ErlangVMA
{
	public class MvcApplication : HttpApplication
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("VirtualMachineConsole.svc");

			routes.MapRoute(
				"Default",
				"{controller}/{action}/{id}",
				new { controller = "Home", action = "Index", id = "" }
			);
		}

		protected void Application_Start()
		{
//			var startup = new Startup();
//			var routeHandler = new OwinRouteHandler(startup.Configuration);
//			RouteTable.Routes.Add("signalr", new Route("signalr/{*params}", routeHandler));

			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);
		}
	}
}
