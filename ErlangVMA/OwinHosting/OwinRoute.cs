using System;
using System.Web.Routing;
using Owin;
using Owin.Builder;
using System.Web;

namespace ErlangVMA
{
	public class OwinRoute : RouteBase
	{
		private AppBuilder builder;

		public OwinRoute ()
		{
			builder = new AppBuilder();
		}

		public IAppBuilder Builder
		{
			get
			{
				return builder;
			}
		}

		public override RouteData GetRouteData (System.Web.HttpContextBase httpContext)
		{
			var routeData = new RouteData(this, new OwinRouteHandler(Builder));
			return routeData;
		}

		public override VirtualPathData GetVirtualPath (RequestContext requestContext, RouteValueDictionary values)
		{
			return null;
		}
	}
}

