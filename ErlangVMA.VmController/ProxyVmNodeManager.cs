using ErlangVMA.TerminalEmulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace ErlangVMA.VmController
{
    public class ProxyVmNodeManager : IVmNodeManager
    {
        private DuplexChannelFactory<IVmNodeManagerService> channelFactory;
        private IVmNodeManagerService client;

        public ProxyVmNodeManager(string endpointConfigurationName)
        {
            this.channelFactory = new DuplexChannelFactory<IVmNodeManagerService>(new VmNodeEventListener(this), endpointConfigurationName);
            this.client = GetClient();
        }

        public ProxyVmNodeManager(string endpointConfigurationName, string endpointAddress)
        {
            this.channelFactory = new DuplexChannelFactory<IVmNodeManagerService>(new VmNodeEventListener(this), endpointConfigurationName, new EndpointAddress(endpointAddress));
            this.client = GetClient();
        }

        public event Action<VmNodeId, ScreenData> ScreenUpdated;

        public VmNodeId StartNewNode()
        {
            return client.StartNewNode();
        }

        public bool IsNodeAlive(VmNodeId address)
        {
            return client.IsNodeAlive(address);
        }

        public void ShutdownNode(VmNodeId address)
        {
            client.ShutdownNode(address);
        }

        public void SendInput(VmNodeId address, IEnumerable<byte> symbols)
        {
            client.SendInput(address, symbols);
        }

        public ScreenData GetScreen(VmNodeId nodeId)
        {
            return client.GetScreen(nodeId);
        }

        private IVmNodeManagerService GetClient()
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
