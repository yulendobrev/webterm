using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ErlangVMA.TerminalEmulation;
using ErlangVMA.VmController;

namespace ErlangVMA.Web
{
    public class VirtualMachineCommunicationBroker
    {
        private readonly IVmBroker vmBroker;

        private readonly ConcurrentDictionary<VmUser, List<ClientConnection>> clientsDict;
        private readonly ConcurrentDictionary<string, List<int>> screenUpdateRegistrations;

        public VirtualMachineCommunicationBroker(IVmBroker vmBroker)
        {
            this.vmBroker = vmBroker;
            this.vmBroker.ScreenUpdated += SendScreenUpdate;

            this.clientsDict = new ConcurrentDictionary<VmUser, List<ClientConnection>>();
            this.screenUpdateRegistrations = new ConcurrentDictionary<string, List<int>>();
        }

        public void SendScreenUpdate(VmUser user, int virtualMachineId, ScreenUpdate screenUpdate)
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
        }

        public void RegisterClient(VmUser user, string connectionId, dynamic client)
        {
            var clients = clientsDict.GetOrAdd(user, u => new List<ClientConnection>());
            lock (clients)
            {
                clients.Add(new ClientConnection(connectionId, client));
            }
        }

        public void UnregisterClient(VmUser user, string connectionId)
        {
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
        }

        public void RegisterForScreenUpdates(string connectionId, int virtualMachineId)
        {
            var virtualMachineIds = screenUpdateRegistrations.GetOrAdd(connectionId, id => new List<int>());
            lock (virtualMachineIds)
            {
                virtualMachineIds.Add(virtualMachineId);
            }
        }

        public void UnregisterFromScreenUpdates(string connectionId, int virtualMachineId)
        {
            List<int> virtualMachineIds;
            if (screenUpdateRegistrations.TryGetValue(connectionId, out virtualMachineIds))
            {
                lock (virtualMachineIds)
                {
                    virtualMachineIds.Remove(virtualMachineId);
                }
            }
        }

        private bool IsClientRegisteredForScreenUpdates(string connectionId, int virtualMachineId)
        {
            List<int> virtualMachineIds;
            if (screenUpdateRegistrations.TryGetValue(connectionId, out virtualMachineIds))
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