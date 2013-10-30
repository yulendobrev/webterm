using System;

namespace ErlangVMA.VmController
{
	public class VmNodeAddress
	{
		private VmHostAddress hostAddress;
		private VmNodeId nodeId;

		public VmNodeAddress() : this(null, null)
		{
		}

		public VmNodeAddress(VmHostAddress hostAddress, VmNodeId nodeId)
		{
			this.hostAddress = hostAddress;
			this.nodeId = nodeId;
		}

		public VmNodeId NodeId
		{
			get { return nodeId; }
			set { nodeId = value; }
		}

		public VmHostAddress HostAddress
		{
			get { return hostAddress; }
			set { hostAddress = value; }
		}

		public override bool Equals(object obj)
		{
			var address = obj as VmNodeAddress;
			return (object)address != null && NodeId == address.NodeId && HostAddress == address.HostAddress;
		}

		public override int GetHashCode()
		{
			int nodeIdHash = NodeId != null ? NodeId.GetHashCode() : 0;
			int hostAddressHash = HostAddress != null ? HostAddress.GetHashCode() : 0;

			return nodeIdHash ^ hostAddressHash;
		}

		public static bool operator==(VmNodeAddress nodeAddress, VmNodeAddress otherNodeAddress)
		{
			return (object)nodeAddress == null && (object)otherNodeAddress == null || (object)nodeAddress != null && nodeAddress.Equals(otherNodeAddress);
		}

		public static bool operator!=(VmNodeAddress nodeAddress, VmNodeAddress otherNodeAddress)
		{
			return !(nodeAddress == otherNodeAddress);
		}
	}
}
