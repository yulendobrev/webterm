using System;
using System.Web;
using Owin.Builder;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin;
using System.Net;
using System.Linq;

namespace ErlangVMA
{
	public class OwinHttpHandler : IHttpAsyncHandler
	{
		private IAppBuilder builder;

		public OwinHttpHandler(IAppBuilder builder)
		{
			this.builder = builder;
		}

		public bool IsReusable { get { return true; } }

		public void ProcessRequest(HttpContext context)
		{
			BeginProcessRequest(context, ar => EndProcessRequest(ar), null);
		}

		public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
		{
			var pipeline = (Func<IDictionary<string, object>, Task>)builder.Build(typeof(Func<IDictionary<string, object>, Task>));
			var owinEnvironment = (builder.Properties as OwinEnvironment).CreateRequestEnvironment(context);

			var task = pipeline(owinEnvironment);

			task.ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
					context.Response.ClearContent();
					context.Response.Output.Write(string.Join("\n\n", t.Exception.InnerExceptions.Select(ex => string.Format("{0}\n{1}", ex.Message, ex.StackTrace)).ToArray()));
					//context.Response.ContentType = "text/plain";
				}

				if (cb != null)
				{
					cb(t);
				}
			});
			
			return task;
		}

		public void EndProcessRequest(IAsyncResult result)
		{
			((Task)result).Wait();
		}
	}
}

