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
            return CreateClient().StartNewNode();
        }

        public bool IsNodeAlive(VmNodeId address)
        {
            return CreateClient().IsNodeAlive(address);
        }

        public void ShutdownNode(VmNodeId address)
        {
            CreateClient().ShutdownNode(address);
        }

        public void SendInput(VmNodeId address, IEnumerable<byte> symbols)
        {
            CreateClient().SendInput(address, symbols);
        }

        public ScreenData GetScreen(VmNodeId nodeId)
        {
            return CreateClient().GetScreen(nodeId);
        }

        private IVmNodeManagerService CreateClient()
        {
            var channel = channelFactory.CreateChannel();
            return channel;
        }

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
