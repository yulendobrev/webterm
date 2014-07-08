using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using ErlangVMA.VmController;
using System.Collections.Generic;
using ErlangVMA.TerminalEmulation;

namespace ErlangVMA
{
    [Authorize]
    public class VirtualMachineHub : Hub
    {
        private static Dictionary<VmUser, dynamic> clientsDict = new Dictionary<VmUser, dynamic>();
        private IVmBroker vmBroker;

        public VirtualMachineHub(IVmBroker vmBroker)
            : base()
        {
            this.vmBroker = vmBroker;
            this.vmBroker.ScreenUpdated += (user, nodeAddress, screenData) =>
            {
                var client = clientsDict[user];
                client.updateScreen(nodeAddress.NodeId, screenData);
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

        public IEnumerable<VmNodeAddress> GetVirtualMachines()
        {
            var user = GetUser();
            var virtualMachines = vmBroker.GetVirtualMachines(user);

            return virtualMachines;
        }

        public VmNodeAddress StartNewNode()
        {
            var user = GetUser();
            var nodeAddress = vmBroker.StartNewNode(user);

            return nodeAddress;
        }

        public void ShutdownNode(VmNodeAddress address)
        {
            var user = GetUser();
            vmBroker.ShutdownNode(user, address);
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
    }
}
