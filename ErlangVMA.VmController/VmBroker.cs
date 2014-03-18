using System;
using System.Collections.Generic;
using System.Linq;
using ErlangVMA.TerminalEmulation;

namespace ErlangVMA.VmController
{
	public class VmBroker : IVmBroker
	{
		private IVmNodeManager nodeManager;
		private List<VmNodeEntry> entries;

		public VmBroker(IVmNodeManager nodeManager)
		{
			this.nodeManager = nodeManager;
			this.entries = new List<VmNodeEntry>();

			nodeManager.ScreenUpdated += OnScreenUpdated;
		}

		public event Action<VmUser, VmNodeAddress, ScreenData> ScreenUpdated;

		public IEnumerable<VmNodeAddress> GetVirtualMachines(VmUser user)
		{
			return entries.Where(e => e.user == user).Select(e => e.address).ToList();
		}

		public VmNodeAddress StartNewNode(VmUser user)
		{
			var nodeId = nodeManager.StartNewNode();

			var address = new VmNodeAddress(VmHostAddress.Local, nodeId);
			entries.Add(new VmNodeEntry() { user = user, address = address });

			return address;
		}

		public VmNodeAddress StartNewNode(VmUser user, VmHostAddress host)
		{
			if (host != VmHostAddress.Local)
			{
				throw new ArgumentException("Starting of new nodes allowed only on localhost", "host");
			}

			return StartNewNode(user);
		}

		public void ShutdownNode(VmUser user, VmNodeAddress address)
		{
			var entry = GetNodeEntry(user, address);
			if (entry != null)
			{
				nodeManager.ShutdownNode(address.NodeId);

				entries.Remove(entry);
			}
		}

		public void SendInput(VmUser user, VmNodeAddress address, IEnumerable<byte> symbols)
		{
			var entry = GetNodeEntry(user, address);
			if (entry != null)
			{
				nodeManager.SendInput(address.NodeId, symbols);
			}
		}

		public ScreenData GetScreen(VmUser user, VmNodeAddress address)
		{
			var entry = GetNodeEntry(user, address);
			if (entry != null)
			{
				var screen = nodeManager.GetScreen(address.NodeId);
				return screen;
			}

			return null;
		}
		
		private void OnScreenUpdated(VmNodeId nodeId, ScreenData screenData)
		{
			var nodeEntries = GetNodeEntries(new VmNodeAddress(VmHostAddress.Local, nodeId));
			foreach (var nodeEntry in nodeEntries)
			{
				RaiseOnScreenUpdated(nodeEntry.user, nodeEntry.address, screenData);
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

		private IEnumerable<VmNodeEntry> GetNodeEntries(VmNodeAddress address)
		{
			var matchingEntries = entries.Where(e => e.address == address).ToList();
			return matchingEntries;
		}

		private VmNodeEntry GetNodeEntry(VmUser user, VmNodeAddress address)
		{
			var entry = entries.FirstOrDefault(e => e.user == user && e.address == address);
			return entry;
		}

		private class VmNodeEntry
		{
			public VmUser user;
			public VmNodeAddress address;
		}
	}
}
