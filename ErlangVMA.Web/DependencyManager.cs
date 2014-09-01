using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ErlangVMA.TerminalEmulation;
using ErlangVMA.VmController;
using Ninject;

namespace ErlangVMA.Web
{
    public class DependencyManager : IDependencyResolver
    {
        private static readonly Lazy<DependencyManager> LazyInstance = new Lazy<DependencyManager>(() => new DependencyManager());

        private IKernel kernel;

        public static DependencyManager Instance
        {
            get { return LazyInstance.Value; }
        }

        protected DependencyManager()
            : this(new StandardKernel())
        {
        }

        protected DependencyManager(IKernel kernel)
        {
            this.kernel = kernel;

            RegisterBindings();
        }

        public object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }

        private void RegisterBindings()
        {
            kernel.Bind<IVmBroker>()
                  .To<VmBroker>()
                  .InSingletonScope();

            kernel.Bind<IVmNodeManager>()
                  .To<ProxyVmNodeManager>()
                  .WithConstructorArgument("endpointConfigurationName", "vmNodeManagerEndpoint");

            kernel.Bind<VirtualMachineCommunicationBroker>()
                  .ToSelf()
                  .InSingletonScope();

            //kernel.Bind<ITerminalEmulatorFactory>().To<UnixTerminalEmulatorFactory>()
            //    .WithConstructorArgument("executablePath", "/bin/bash")
            //    .WithConstructorArgument("arguments", new[] { "-i" });
        }
    }
}