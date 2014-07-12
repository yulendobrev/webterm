using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ErlangVMA.TerminalEmulation;
using ErlangVMA.VmController;
using Microsoft.AspNet.SignalR;

namespace ErlangVMA
{
    [Authorize]
    public class VirtualMachineHub : Hub
    {
        private static Dictionary<VmUser, dynamic> clientsDict = new Dictionary<VmUser, dynamic>();
        private static ConcurrentDictionary<VmUser, List<VmNodeAddress>> screenUpdateRegistrations = new ConcurrentDictionary<VmUser, List<VmNodeAddress>>();
        private IVmBroker vmBroker;

        public VirtualMachineHub(IVmBroker vmBroker)
            : base()
        {
            this.vmBroker = vmBroker;
            this.vmBroker.ScreenUpdated += (user, nodeAddress, screenData) =>
            {
                dynamic client;
                if (clientsDict.TryGetValue(user, out client) &&
                    IsUserRegisteredForScreenUpdates(user, nodeAddress))
                {

                    client.updateScreen(nodeAddress.NodeId, screenData);
                }
            };
        }

        public override Task OnConnected()
        {
            return base.OnConnected().ContinueWith(t =>
            {
                var client = Clients.Caller;
                var user = GetUser();

                clientsDict[user] = client;
            });
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled).ContinueWith(t =>
            {
                var user = GetUser();

                clientsDict.Remove(user);
            });
        }

        public void RegisterForScreenUpdates(VmNodeAddress nodeAddress)
        {
            var nodeAddresses = screenUpdateRegistrations.GetOrAdd(GetUser(), user => new List<VmNodeAddress>());
            lock (nodeAddresses)
            {
                nodeAddresses.Add(nodeAddress);
            }
        }

        public void UnregisterFromScreenUpdates(VmNodeAddress nodeAddress)
        {
            List<VmNodeAddress> nodeAddresses;
            if (screenUpdateRegistrations.TryGetValue(GetUser(), out nodeAddresses))
            {
                lock (nodeAddresses)
                {
                    nodeAddresses.Remove(nodeAddress);
                }
            }
        }

        public void ProcessInput(VmNodeAddress nodeAddress, byte input)
        {
            ProcessChunkInput(nodeAddress, new[] { input });
        }

        public void ProcessChunkInput(VmNodeAddress nodeAddress, byte[] input)
        {
            var user = GetUser();
            vmBroker.SendInput(user, nodeAddress, input);
        }

        public ScreenData GetScreen(VmNodeAddress address)
        {
            var user = GetUser();
            var screen = vmBroker.GetScreen(user, address);
            return screen;
        }

        private VmUser GetUser()
        {
            string username = Context.User.Identity.Name;
            var user = new VmUser(username);

            return user;
        }

        private bool IsUserRegisteredForScreenUpdates(VmUser user, VmNodeAddress nodeAddress)
        {
            List<VmNodeAddress> nodeAddresses;
            if (screenUpdateRegistrations.TryGetValue(user, out nodeAddresses))
            {
                lock (nodeAddresses)
                {
                    return nodeAddresses.Contains(nodeAddress);
                }
            }

            return false;
        }
    }
}
