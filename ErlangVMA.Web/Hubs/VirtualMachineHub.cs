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
        private static ConcurrentDictionary<VmUser, List<ClientConnection>> clientsDict = new ConcurrentDictionary<VmUser, List<ClientConnection>>();
        private static ConcurrentDictionary<string, List<int>> screenUpdateRegistrations = new ConcurrentDictionary<string, List<int>>();
        private IVmBroker vmBroker;

        public VirtualMachineHub(IVmBroker vmBroker)
            : base()
        {
            this.vmBroker = vmBroker;
            this.vmBroker.ScreenUpdated += (user, virtualMachineId, screenUpdate) =>
            {
                List<ClientConnection> clients;
                if (clientsDict.TryGetValue(user, out clients))
                {
                    var clientsSnapshot = new List<ClientConnection>();
                    lock (clients)
                    {
                        clientsSnapshot.AddRange(clients);
                    }

                    foreach (var client in clientsSnapshot)
                    {
                        if (IsClientRegisteredForScreenUpdates(client.ConnectionId, virtualMachineId))
                        {
                            client.Client.updateScreen(virtualMachineId, screenUpdate);
                        }
                    }
                }
            };
        }

        public override Task OnConnected()
        {
            return base.OnConnected().ContinueWith(t =>
            {
                var user = GetUser();

                var clients = clientsDict.GetOrAdd(user, u => new List<ClientConnection>());
                lock (clients)
                {
                    clients.Add(new ClientConnection(Context.ConnectionId, Clients.Caller));
                }
            });
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled).ContinueWith(t =>
            {
                string connectionId = Context.ConnectionId;
                var user = GetUser();

                List<ClientConnection> clients;
                if (clientsDict.TryGetValue(user, out clients))
                {
                    bool isLastClientRemoved = false;
                    lock (clients)
                    {
                        clients.RemoveAll(c => c.ConnectionId == connectionId);
                        isLastClientRemoved = clients.Count == 0;
                    }

                    if (isLastClientRemoved)
                    {
                        List<ClientConnection> ignored;
                        clientsDict.TryRemove(user, out ignored);
                    }
                }
            });
        }

        public void RegisterForScreenUpdates(int virtualMachineId)
        {
            var virtualMachineIds = screenUpdateRegistrations.GetOrAdd(Context.ConnectionId, connectionId => new List<int>());
            lock (virtualMachineIds)
            {
                virtualMachineIds.Add(virtualMachineId);
            }
        }

        public void UnregisterFromScreenUpdates(int virtualMachineId)
        {
            List<int> virtualMachineIds;
            if (screenUpdateRegistrations.TryGetValue(Context.ConnectionId, out virtualMachineIds))
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

        private bool IsClientRegisteredForScreenUpdates(string clientId, int virtualMachineId)
        {
            List<int> virtualMachineIds;
            if (screenUpdateRegistrations.TryGetValue(clientId, out virtualMachineIds))
            {
                lock (virtualMachineIds)
                {
                    return virtualMachineIds.Contains(virtualMachineId);
                }
            }

            return false;
        }

        private class ClientConnection
        {
            private readonly string connectionId;
            private readonly dynamic client;

            public ClientConnection(string connectionId, dynamic client)
            {
                this.connectionId = connectionId;
                this.client = client;
            }

            public string ConnectionId
            {
                get { return connectionId; }
            }

            public dynamic Client
            {
                get { return client; }
            }

            public override bool Equals(object obj)
            {
                var otherClient = obj as ClientConnection;
                return otherClient != null && connectionId.Equals(otherClient.connectionId);
            }

            public override int GetHashCode()
            {
                return connectionId != null ? connectionId.GetHashCode() : 0;
            }
        }
    }
}
