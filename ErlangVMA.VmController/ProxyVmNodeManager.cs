using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using ErlangVMA.TerminalEmulation;

namespace ErlangVMA.VmController
{
    public class ProxyVmNodeManager : IVmNodeManager
    {
        private DuplexChannelFactory<IVmNodeManagerService> channelFactory;
        private IVmNodeManagerService channel;

        public ProxyVmNodeManager(string endpointConfigurationName)
        {
            this.channelFactory = new DuplexChannelFactory<IVmNodeManagerService>(new VmNodeEventListener(this), endpointConfigurationName);
        }

        public ProxyVmNodeManager(string endpointConfigurationName, string endpointAddress)
        {
            this.channelFactory = new DuplexChannelFactory<IVmNodeManagerService>(new VmNodeEventListener(this), endpointConfigurationName, new EndpointAddress(endpointAddress));
        }

        public event Action<VmNodeId, ScreenData> ScreenUpdated;

        public VmNodeId StartNewNode()
        {
            return ExecuteWithClient(c => c.StartNewNode());
        }

        public bool IsNodeAlive(VmNodeId address)
        {
            return ExecuteWithClient(c => c.IsNodeAlive(address));
        }

        public void ShutdownNode(VmNodeId address)
        {
            ExecuteWithClient(c => c.ShutdownNode(address));
        }

        public void SendInput(VmNodeId address, IEnumerable<byte> symbols)
        {
            ExecuteWithClient(c => c.SendInput(address, symbols));
        }

        public ScreenData GetScreen(VmNodeId nodeId)
        {
            return ExecuteWithClient(c => c.GetScreen(nodeId));
        }

        private void ExecuteWithClient(Action<IVmNodeManagerService> action)
        {
            ExecuteWithClient(s =>
            {
                action(s);
                return 0;
            });
        }

        private T ExecuteWithClient<T>(Func<IVmNodeManagerService, T> func)
        {
            if (channel == null)
            {
                channel = channelFactory.CreateChannel(new InstanceContext(new VmNodeEventListener(this)));
            }

            var duplexChannel = channel as IDuplexContextChannel;
            try
            {
                return func(channel);
            }
            catch (TimeoutException)
            {
                duplexChannel.Abort();
                channel = null;
                throw;
            }
            catch (CommunicationException)
            {
                duplexChannel.Abort();
                channel = null;
                throw;
            }
        }

        [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false, IncludeExceptionDetailInFaults = true)]
        private class VmNodeEventListener : IVmNodeEventListener
        {
            private readonly ProxyVmNodeManager vmNodeManager;

            public VmNodeEventListener(ProxyVmNodeManager vmNodeManager)
            {
                this.vmNodeManager = vmNodeManager;
            }

            public void ScreenUpdated(VmNodeId nodeId, ScreenData screenData)
            {
                var screenUpdatedHandler = vmNodeManager.ScreenUpdated;
                if (screenUpdatedHandler != null)
                {
                    screenUpdatedHandler(nodeId, screenData);
                }
            }
        }
    }
}
