using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ErlangVMA.Web
{
    public class SignalRDependencyResolver : DefaultDependencyResolver
    {
        private readonly System.Web.Mvc.IDependencyResolver dependencyResolver;

        public SignalRDependencyResolver(System.Web.Mvc.IDependencyResolver dependencyResolver)
        {
            this.dependencyResolver = dependencyResolver;
        }

        public override object GetService(Type serviceType)
        {
            return dependencyResolver.GetService(serviceType) ?? base.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            return dependencyResolver.GetServices(serviceType).Concat(base.GetServices(serviceType));
        }
    }
}