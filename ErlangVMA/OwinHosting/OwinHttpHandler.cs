using System;
using System.Web;
using Owin.Builder;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin;

namespace ErlangVMA
{
	public class OwinHttpHandler : IHttpAsyncHandler
	{
		private IAppBuilder builder;

		public OwinHttpHandler (IAppBuilder builder)
		{
			this.builder = builder;
		}

		public bool IsReusable { get { return true; } }

		public void ProcessRequest (HttpContext context)
		{
			BeginProcessRequest(context, ar => EndProcessRequest(ar), null);
		}

		public IAsyncResult BeginProcessRequest (HttpContext context, AsyncCallback cb, object extraData)
		{
			var pipeline = (Func<IDictionary<string, object>, Task>)builder.Build (typeof(Func<IDictionary<string, object>, Task>));
			var owinEnvironment = (builder.Properties as OwinEnvironment).CreateRequestEnvironment (context);

			var task = pipeline (owinEnvironment);

			task.ContinueWith (t => {
				if (cb != null)
					cb (t);
			});
			
			return task;
		}

		public void EndProcessRequest (IAsyncResult result)
		{
			((Task)result).Wait ();
		}
	}
}

