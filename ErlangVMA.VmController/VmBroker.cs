using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ErlangVMA.TerminalEmulation;
using ErlangVMA.VmController.Persistence;

namespace ErlangVMA.VmController
{
    public class VmBroker : IVmBroker
    {
        private readonly IVmNodeManager nodeManager;
        private readonly VmNodesDbModel vmNodesDbModel;
        private static readonly object vmNodesDbModelLock = new object();

        public VmBroker(IVmNodeManager nodeManager, VmNodesDbModel vmNodesDbModel)
        {
            this.nodeManager = nodeManager;
            this.vmNodesDbModel = vmNodesDbModel;

            nodeManager.ScreenUpdated += OnScreenUpdated;
        }

        public event Action<VmUser, int, ScreenData> ScreenUpdated;

        public IEnumerable<VirtualMachine> GetVirtualMachines(VmUser user)
        {
            var virtualMachines = new List<VirtualMachine>();

            IEnumerable<VmNodeDbEntry> userNodes;
            lock (vmNodesDbModelLock)
            {
                userNodes = vmNodesDbModel.ActiveVmNodes.Where(n => n.User == user.Username).ToList();
            }
            foreach (var node in userNodes)
            {
                var virtualMachine = new VirtualMachine();

                virtualMachine.Name = node.Name;
                virtualMachine.NodeAddress = new VmNodeAddress(new VmHostAddress(IPAddress.Parse(node.HostMachine)), new VmNodeId(node.VmNodeId));
                virtualMachine.StartedOn = node.StartedOn;
                //virtualMachine.IsActive = nodeManager.IsNodeAlive(new VmNodeId(node.VmNodeId));

                virtualMachines.Add(virtualMachine);
            }

            return virtualMachines;
        }

        public int StartNewNode(VmUser user, VirtualMachineStartOptions startOptions)
        {
            var nodeId = nodeManager.StartNewNode();
            var address = new VmNodeAddress(VmHostAddress.Local, nodeId);

            var vmNodeDbEntry = new VmNodeDbEntry
            {
                Name = startOptions.Name,
                User = user.Username,
                HostMachine = VmHostAddress.Local.Ip.ToString(),
                VmNodeId = nodeId.Id,
                StartedOn = DateTime.Now
            };
            lock (vmNodesDbModelLock)
            {
                vmNodesDbModel.ActiveVmNodes.Add(vmNodeDbEntry);
                vmNodesDbModel.SaveChanges();
            }

            return vmNodeDbEntry.Id;
        }

        public int StartNewNode(VmUser user, VmHostAddress host, VirtualMachineStartOptions startOptions)
        {
            if (host != VmHostAddress.Local)
            {
                throw new ArgumentException("Starting of new nodes allowed only on localhost", "host");
            }

            return StartNewNode(user, startOptions);
        }

        public void ShutdownNode(VmUser user, int virtualMachineId)
        {
            var entry = GetVmNodeDbEntry(user, virtualMachineId);
            if (entry != null)
            {
                var address = GetAddress(entry);
                nodeManager.ShutdownNode(address.NodeId);

                lock (vmNodesDbModelLock)
                {
                    vmNodesDbModel.ActiveVmNodes.Remove(entry);
                    vmNodesDbModel.SaveChanges();
                }
            }
        }

        public void SendInput(VmUser user, int virtualMachineId, IEnumerable<byte> symbols)
        {
            var entry = GetVmNodeDbEntry(user, virtualMachineId);
            if (entry != null)
            {
                var address = GetAddress(entry);
                nodeManager.SendInput(address.NodeId, symbols);
            }
        }

        public ScreenData GetScreen(VmUser user, int virtualMachineId)
        {
            var entry = GetVmNodeDbEntry(user, virtualMachineId);
            if (entry != null)
            {
                var address = GetAddress(entry);
                var screen = nodeManager.GetScreen(address.NodeId);

                return screen;
            }

            return null;
        }

        private void OnScreenUpdated(VmNodeId nodeId, ScreenData screenData)
        {
            var vmNodeAddress = new VmNodeAddress(VmHostAddress.Local, nodeId);
            var nodeEntries = GetVmNodeDbEntries(vmNodeAddress);
            foreach (var nodeEntry in nodeEntries)
            {
                RaiseOnScreenUpdated(new VmUser(nodeEntry.User), nodeEntry.Id, screenData);
            }
        }

        private void RaiseOnScreenUpdated(VmUser user, int virtualMachineId, ScreenData screenData)
        {
            var screenUpdatedHandler = ScreenUpdated;
            if (screenUpdatedHandler != null)
            {
                screenUpdatedHandler(user, virtualMachineId, screenData);
            }
        }

        private IEnumerable<VmNodeDbEntry> GetVmNodeDbEntries(VmNodeAddress address)
        {
            string hostMachine = address.HostAddress.Ip.ToString();
            lock (vmNodesDbModelLock)
            {
                var matchingEntries = vmNodesDbModel.ActiveVmNodes.Where(n => n.HostMachine == hostMachine && n.VmNodeId == address.NodeId.Id).ToList();
                return matchingEntries;
            }
        }

        private VmNodeDbEntry GetVmNodeDbEntry(VmUser user, int id)
        {
            lock (vmNodesDbModelLock)
            {
                var dbNodeEntry = vmNodesDbModel.ActiveVmNodes.Find(id);
                if (dbNodeEntry.User != user.Username)
                {
                    return null;
                }
                return dbNodeEntry;
            }
        }

        private VmNodeAddress GetAddress(VmNodeDbEntry entry)
        {
            return new VmNodeAddress(new VmHostAddress(IPAddress.Parse(entry.HostMachine)), new VmNodeId(entry.VmNodeId));
        }
    }
}
