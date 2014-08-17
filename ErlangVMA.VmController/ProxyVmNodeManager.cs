﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using ErlangVMA.TerminalEmulation;

namespace ErlangVMA.VmController
{
    public class ProxyVmNodeManager : IVmNodeManager
    {
        private ChannelFactory<IVmNodeManagerService> channelFactory;
        private IVmNodeManagerService channel;
        private DuplexVmInteractionClient duplexClient;

        public ProxyVmNodeManager(string endpointConfigurationName)
        {
            this.channelFactory = new ChannelFactory<IVmNodeManagerService>(endpointConfigurationName);
            this.duplexClient = new DuplexVmInteractionClient(RaiseScreenUpdated, IPAddress.Parse("192.168.122.1"), 4300);

            duplexClient.InteractAsync();
        }

        public ProxyVmNodeManager(string endpointConfigurationName, string endpointAddress)
        {
            this.channelFactory = new ChannelFactory<IVmNodeManagerService>(endpointConfigurationName, new EndpointAddress(endpointAddress));
            this.duplexClient = new DuplexVmInteractionClient(RaiseScreenUpdated, IPAddress.Parse("192.168.122.1"), 4300);

            duplexClient.InteractAsync();
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
            //ExecuteWithClient(c => c.SendInput(address, symbols));
            duplexClient.SendInput(address, symbols);
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
                channel = channelFactory.CreateChannel();
            }

            var duplexChannel = channel as IClientChannel;
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

        private void RaiseScreenUpdated(VmNodeId nodeId, ScreenData screenData)
        {
            var screenUpdatedHandler = ScreenUpdated;
            if (screenUpdatedHandler != null)
            {
                screenUpdatedHandler(nodeId, screenData);
            }
        }
    }
}
