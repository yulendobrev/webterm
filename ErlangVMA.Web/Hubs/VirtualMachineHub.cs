using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ErlangVMA.TerminalEmulation;
using ErlangVMA.VmController;
using Microsoft.AspNet.SignalR;

namespace ErlangVMA.Web
{
    [Authorize]
    public class VirtualMachineHub : Hub
    {
        private readonly IVmBroker vmBroker;
        private readonly VirtualMachineCommunicationBroker communicationBroker;

        public VirtualMachineHub(IVmBroker vmBroker, VirtualMachineCommunicationBroker communicatioBroker)
            : base()
        {
            this.vmBroker = vmBroker;
            this.communicationBroker = communicatioBroker;
        }

        public override Task OnConnected()
        {
            return base.OnConnected().ContinueWith(t =>
            {
                var user = GetUser();
                this.communicationBroker.RegisterClient(user, Context.ConnectionId, Clients.Caller);
            });
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled).ContinueWith(t =>
            {
                var user = GetUser();
                this.communicationBroker.UnregisterClient(user, Context.ConnectionId);
            });
        }

        public void RegisterForScreenUpdates(int virtualMachineId)
        {
            this.communicationBroker.RegisterForScreenUpdates(Context.ConnectionId, virtualMachineId);
        }

        public void UnregisterFromScreenUpdates(int virtualMachineId)
        {
            this.communicationBroker.UnregisterFromScreenUpdates(Context.ConnectionId, virtualMachineId);
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
    }
}
