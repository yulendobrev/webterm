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
        private static ConcurrentDictionary<VmUser, List<int>> screenUpdateRegistrations = new ConcurrentDictionary<VmUser, List<int>>();
        private IVmBroker vmBroker;

        public VirtualMachineHub(IVmBroker vmBroker)
            : base()
        {
            this.vmBroker = vmBroker;
            this.vmBroker.ScreenUpdated += (user, virtualMachineId, screenData) =>
            {
                dynamic client;
                if (clientsDict.TryGetValue(user, out client) &&
                    IsUserRegisteredForScreenUpdates(user, virtualMachineId))
                {

                    client.updateScreen(virtualMachineId, screenData);
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

        public void RegisterForScreenUpdates(int virtualMachineId)
        {
            var virtualMachineIds = screenUpdateRegistrations.GetOrAdd(GetUser(), user => new List<int>());
            lock (virtualMachineIds)
            {
                virtualMachineIds.Add(virtualMachineId);
            }
        }

        public void UnregisterFromScreenUpdates(int virtualMachineId)
        {
            List<int> virtualMachineIds;
            if (screenUpdateRegistrations.TryGetValue(GetUser(), out virtualMachineIds))
            {
                lock (virtualMachineIds)
                {
                    virtualMachineIds.Remove(virtualMachineId);
                }
            }
        }

        public void ProcessInput(int virtualMachineId, byte input)
        {
            ProcessChunkInput(virtualMachineId, new[] { input });
        }

        public void ProcessChunkInput(int virtualMachineId, byte[] input)
        {
            var user = GetUser();
            vmBroker.SendInput(user, virtualMachineId, input);
        }

        public ScreenData GetScreen(int virtualMachineId)
        {
            var user = GetUser();
            var screen = vmBroker.GetScreen(user, virtualMachineId);

            return screen;
        }

        private VmUser GetUser()
        {
            string username = Context.User.Identity.Name;
            var user = new VmUser(username);

            return user;
        }

        private bool IsUserRegisteredForScreenUpdates(VmUser user, int virtualMachineId)
        {
            List<int> virtualMachineIds;
            if (screenUpdateRegistrations.TryGetValue(user, out virtualMachineIds))
            {
                lock (virtualMachineIds)
                {
                    return virtualMachineIds.Contains(virtualMachineId);
                }
            }

            return false;
        }
    }
}
