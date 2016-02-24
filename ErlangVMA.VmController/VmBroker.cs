using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ErlangVMA.TerminalEmulation;
using ErlangVMA.VmController.Persistence;

namespace ErlangVMA.VmController
{
    public class VmBroker : IVmBroker
    {
        private List<ExecutionEngineMachine> machines;
        private ConcurrentDictionary<VmHostAddress, IVmNodeManager> nodeManagers;

        public VmBroker(IEnumerable<ExecutionEngineMachine> machines)
        {
            this.machines = machines.ToList();
            this.nodeManagers = new ConcurrentDictionary<VmHostAddress, IVmNodeManager>();

            using (var dbContext = new VmNodesDbContext())
            {
                foreach (var machine in machines)
                {
                    if (dbContext.ExecutionMachineLoads.Find(machine.IpAddress) == null)
                    {
                        dbContext.ExecutionMachineLoads.Add(new ExecutionMachineLoad { Address = machine.IpAddress });
                    }
                }

                dbContext.SaveChanges();
            }
        }

        public event Action<VmUser, int, ScreenUpdate> ScreenUpdated;
        
        public IEnumerable<VirtualMachine> GetVirtualMachines(VmUser user)
        {
            var virtualMachines = new List<VirtualMachine>();

            IEnumerable<VmNodeEntry> userNodes;
            using (var dbContext = new VmNodesDbContext())
            {
                userNodes = dbContext.ActiveVmNodes.AsNoTracking().Where(n => n.User == user.Username).ToList();
            }
            foreach (var node in userNodes)
            {
                var virtualMachine = new VirtualMachine();

                virtualMachine.Id = node.Id;
                virtualMachine.Name = node.Name;
                virtualMachine.NodeAddress = GetAddress(node);
                virtualMachine.StartedOn = node.StartedOn;

                var nodeManager = GetNodeManager(virtualMachine.NodeAddress);
                virtualMachine.IsActive = nodeManager.IsNodeAlive(virtualMachine.NodeAddress.NodeId);

                virtualMachines.Add(virtualMachine);
            }

            return virtualMachines;
        }

        public int StartNewNode(VmUser user, VirtualMachineStartOptions startOptions)
        {
            ExecutionEngineMachine startingMachine = null;
            //if (this.machines.Count == 1)
            //{
            //    startingMachine = this.machines[0];
            //}
            //else
            //{
                using (var dbContext = new VmNodesDbContext())
                {
                    var address = dbContext.ExecutionMachineLoads.OrderBy(l => l.VirtualMachineCount).Select(l => l.Address).FirstOrDefault();
                    startingMachine = this.machines.FirstOrDefault(m => m.IpAddress == address);
                }
            //}

            return StartNewNode(user, new VmHostAddress(IPAddress.Parse(startingMachine.IpAddress)), startOptions);
        }

        public int StartNewNode(VmUser user, VmHostAddress host, VirtualMachineStartOptions startOptions)
        {
            var nodeManager = GetNodeManager(host);

            var nodeId = nodeManager.StartNewNode();

            var vmNodeDbEntry = new VmNodeEntry
            {
                Name = startOptions.Name,
                User = user.Username,
                HostMachine = host.Ip.ToString(),
                VmNodeId = nodeId.Id,
                StartedOn = DateTime.Now
            };
            using (var dbContext = new VmNodesDbContext())
            {
                dbContext.ActiveVmNodes.Add(vmNodeDbEntry);
                ++dbContext.ExecutionMachineLoads.Find(vmNodeDbEntry.HostMachine).VirtualMachineCount;

                dbContext.SaveChanges();
            }

            return vmNodeDbEntry.Id;
        }

        public void ShutdownNode(VmUser user, int virtualMachineId)
        {
            using (var dbContext = new VmNodesDbContext())
            {
                var entry = GetVmNodeDbEntry(dbContext, user, virtualMachineId);
                if (entry != null)
                {
                    var address = GetAddress(entry);
                    var nodeManager = GetNodeManager(address);
                    nodeManager.ShutdownNode(address.NodeId);


                    dbContext.ActiveVmNodes.Remove(entry);
                    --dbContext.ExecutionMachineLoads.Find(entry.HostMachine).VirtualMachineCount;

                    dbContext.SaveChanges();
                }
            }
        }

        public void SendInput(VmUser user, int virtualMachineId, IEnumerable<byte> symbols)
        {
            var entry = GetVmNodeDbEntry(user, virtualMachineId);
            if (entry != null)
            {
                var address = GetAddress(entry);
                var nodeManager = GetNodeManager(address);
                nodeManager.SendInput(address.NodeId, symbols);
            }
        }

        public ScreenData GetScreen(VmUser user, int virtualMachineId)
        {
            var entry = GetVmNodeDbEntry(user, virtualMachineId);
            if (entry != null)
            {
                var address = GetAddress(entry);
                var nodeManager = GetNodeManager(address);
                var screen = nodeManager.GetScreen(address.NodeId);

                return screen;
            }

            return null;
        }

        private void RaiseOnScreenUpdated(VmUser user, int virtualMachineId, ScreenUpdate screenData)
        {
            var screenUpdatedHandler = ScreenUpdated;
            if (screenUpdatedHandler != null)
            {
                screenUpdatedHandler(user, virtualMachineId, screenData);
            }
        }

        private IEnumerable<VmNodeEntry> GetVmNodeDbEntries(VmNodeAddress address)
        {
            string hostMachine = address.HostAddress.Ip.ToString();
            using (var dbContext = new VmNodesDbContext())
            {
                var matchingEntries = dbContext.ActiveVmNodes.Where(n => n.HostMachine == hostMachine && n.VmNodeId == address.NodeId.Id).ToList();
                return matchingEntries;
            }
        }

        private VmNodeEntry GetVmNodeDbEntry(VmNodesDbContext dbContext, VmUser user, int id)
        {
            var dbNodeEntry = dbContext.ActiveVmNodes.Find(id);
            if (dbNodeEntry.User != user.Username)
            {
                return null;
            }
            return dbNodeEntry;
        }

        private VmNodeEntry GetVmNodeDbEntry(VmUser user, int id)
        {
            using (var dbContext = new VmNodesDbContext())
            {
                return GetVmNodeDbEntry(dbContext, user, id);
            }
        }

        private IVmNodeManager GetNodeManager(VmNodeAddress address)
        {
            return GetNodeManager(address.HostAddress);
        }

        private IVmNodeManager GetNodeManager(VmHostAddress host)
        {
            return this.nodeManagers.GetOrAdd(host, h =>
            {
                var machine = this.machines.FirstOrDefault(m => m.IpAddress == host.Ip.ToString());

                var nodeManager = new ProxyVmNodeManager(machine);
                nodeManager.ScreenUpdated += (nodeId, screenData) =>
                {
                    var vmNodeAddress = new VmNodeAddress(h, nodeId);
                    var nodeEntries = GetVmNodeDbEntries(vmNodeAddress);
                    foreach (var nodeEntry in nodeEntries)
                    {
                        RaiseOnScreenUpdated(new VmUser(nodeEntry.User), nodeEntry.Id, screenData);
                    }
                };

                return nodeManager;
            });
        }

        private VmNodeAddress GetAddress(VmNodeEntry entry)
        {
            return new VmNodeAddress(new VmHostAddress(IPAddress.Parse(entry.HostMachine)), new VmNodeId(entry.VmNodeId));
        }
    }
}
