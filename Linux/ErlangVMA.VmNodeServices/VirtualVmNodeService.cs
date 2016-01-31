using System.Collections.Generic;
using System.ServiceModel;
using ErlangVMA.TerminalEmulation;
using ErlangVMA.VmController;

namespace ErlangVMA.VmNodeServices
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class VirtualVmNodeService : IVmNodeManagerService
	{
		private readonly IVmNodeManager vmNodeManager;

		public VirtualVmNodeService(IVmNodeManager vmNodeManager)
		{
			this.vmNodeManager = vmNodeManager;
		}

		public VmNodeId StartNewNode()
		{
			return vmNodeManager.StartNewNode();
		}

		public bool IsNodeAlive(VmNodeId address)
		{
			return vmNodeManager.IsNodeAlive(address);
		}

		public void ShutdownNode(VmNodeId address)
		{
			vmNodeManager.ShutdownNode(address);
		}

		public void SendInput(VmNodeId address, IEnumerable<byte> symbols)
		{
			vmNodeManager.SendInput(address, symbols);
		}

		public Screen GetScreen(VmNodeId nodeId)
		{
			var screenData = vmNodeManager.GetScreen(nodeId);
			return new Screen
			{
				CursorPosition = screenData.CursorPosition,
				X = screenData.X,
				Y = screenData.Y,
				Width = screenData.Width,
				Height = screenData.Height,
				Data = screenData.Data
			};
		}
	}
}
