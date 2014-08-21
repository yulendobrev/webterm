using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ErlangVMA.VmController
{
    public class VirtualizingVmNodeManager : IVmNodeManager
    {
        public VmNodeId StartNewNode()
        {
            throw new NotImplementedException();
        }

        public bool IsNodeAlive(VmNodeId address)
        {
            throw new NotImplementedException();
        }

        public void ShutdownNode(VmNodeId address)
        {
            throw new NotImplementedException();
        }

        public void SendInput(VmNodeId address, IEnumerable<byte> symbols)
        {
            throw new NotImplementedException();
        }

        public event Action<VmNodeId, TerminalEmulation.ScreenUpdate> ScreenUpdated;

        public TerminalEmulation.ScreenData GetScreen(VmNodeId nodeId)
        {
            throw new NotImplementedException();
        }
    }
}
