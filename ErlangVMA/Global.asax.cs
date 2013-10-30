
﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Owin;
using Owin.Builder;

namespace ErlangVMA
{
	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterRoutes (RouteCollection routes)
		{
			routes.IgnoreRoute ("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute ("VirtualMachineConsole.svc");

			routes.MapRoute (
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = "" }
			);

		}

		protected void Application_Start ()
		{
			var routeHandler = new OwinRouteHandler();

			RouteTable.Routes.Add("signalr", new Route("signalr/{*params}", routeHandler));

			(routeHandler.Builder as AppBuilder).Properties = new OwinEnvironment(this);
			routeHandler.Builder.MapHubs();

			AreaRegistration.RegisterAllAreas ();
			RegisterRoutes (RouteTable.Routes);
		}
	}
}
