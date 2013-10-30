using System;
using System.Web.Routing;
using System.Web;
using Owin;
using Owin.Builder;

namespace ErlangVMA
{
	public class OwinRouteHandler : IRouteHandler
	{
		private IAppBuilder builder;

		public OwinRouteHandler () :
			this(new AppBuilder())
		{
		}

		public OwinRouteHandler (IAppBuilder builder)
		{
			this.builder = builder;
		}

		public IAppBuilder Builder
		{
			get
			{
				return builder;
			}
		}

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new OwinHttpHandler(Builder);
		}
	}
}

