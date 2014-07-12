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

        public VmBroker(IVmNodeManager nodeManager, VmNodesDbModel vmNodesDbModel)
        {
            this.nodeManager = nodeManager;
            this.vmNodesDbModel = vmNodesDbModel;

            nodeManager.ScreenUpdated += OnScreenUpdated;
        }

        public event Action<VmUser, VmNodeAddress, ScreenData> ScreenUpdated;

        public IEnumerable<VirtualMachine> GetVirtualMachines(VmUser user)
        {
            var virtualMachines = new List<VirtualMachine>();

            var userNodes = vmNodesDbModel.ActiveVmNodes.Where(n => n.User == user.Username).ToList();
            foreach (var node in userNodes)
            {
                var virtualMachine = new VirtualMachine();

                virtualMachine.Name = node.Name;
                virtualMachine.NodeAddress = new VmNodeAddress(new VmHostAddress(IPAddress.Parse(node.HostMachine)), new VmNodeId(node.VmNodeId));
                virtualMachine.StartedOn = node.StartedOn;
                virtualMachine.IsActive = nodeManager.IsNodeAlive(new VmNodeId(node.VmNodeId));

                virtualMachines.Add(virtualMachine);
            }

            return virtualMachines;
        }

        public VmNodeAddress StartNewNode(VmUser user, VirtualMachineStartOptions startOptions)
        {
            var nodeId = nodeManager.StartNewNode();
            var address = new VmNodeAddress(VmHostAddress.Local, nodeId);

            vmNodesDbModel.ActiveVmNodes.Add(
            new VmNodeDbEntry
            {
                Name = startOptions.Name,
                User = user.Username,
                HostMachine = VmHostAddress.Local.Ip.ToString(),
                VmNodeId = nodeId.NodeId,
                StartedOn = DateTime.Now
            });
            vmNodesDbModel.SaveChanges();

            return address;
        }

        public VmNodeAddress StartNewNode(VmUser user, VmHostAddress host, VirtualMachineStartOptions startOptions)
        {
            if (host != VmHostAddress.Local)
            {
                throw new ArgumentException("Starting of new nodes allowed only on localhost", "host");
            }

            return StartNewNode(user, startOptions);
        }

        public void ShutdownNode(VmUser user, VmNodeAddress address)
        {
            var entry = GetVmNodeDbEntry(user, address);
            if (entry != null)
            {
                nodeManager.ShutdownNode(address.NodeId);

                vmNodesDbModel.ActiveVmNodes.Remove(entry);
                vmNodesDbModel.SaveChanges();
            }
        }

        public void SendInput(VmUser user, VmNodeAddress address, IEnumerable<byte> symbols)
        {
            var entry = GetVmNodeDbEntry(user, address);
            if (entry != null)
            {
                nodeManager.SendInput(address.NodeId, symbols);
            }
        }

        public ScreenData GetScreen(VmUser user, VmNodeAddress address)
        {
            var entry = GetVmNodeDbEntry(user, address);
            if (entry != null)
            {
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
                RaiseOnScreenUpdated(new VmUser(nodeEntry.User), vmNodeAddress, screenData);
            }
        }

        private void RaiseOnScreenUpdated(VmUser user, VmNodeAddress address, ScreenData screenData)
        {
            var screenUpdatedHandler = ScreenUpdated;
            if (screenUpdatedHandler != null)
            {
                screenUpdatedHandler(user, address, screenData);
            }
        }

        private IEnumerable<VmNodeDbEntry> GetVmNodeDbEntries(VmNodeAddress address)
        {
            var matchingEntries = vmNodesDbModel.ActiveVmNodes.Where(n => n.HostMachine == address.HostAddress.Ip.ToString() && n.VmNodeId == address.NodeId.NodeId).ToList();
            return matchingEntries;
        }

        private VmNodeDbEntry GetVmNodeDbEntry(VmUser user, VmNodeAddress address)
        {
            var dbNode = vmNodesDbModel.ActiveVmNodes.FirstOrDefault(n => n.User == user.Username && n.HostMachine == address.HostAddress.Ip.ToString() && n.VmNodeId == address.NodeId.NodeId);
            return dbNode;
        }
    }
}
