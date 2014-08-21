using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ErlangVMA.TerminalEmulation;

namespace ErlangVMA.VmController
{
    public interface IVmNodeManager
    {
        VmNodeId StartNewNode();
        bool IsNodeAlive(VmNodeId address);
        void ShutdownNode(VmNodeId address);
        void SendInput(VmNodeId address, IEnumerable<byte> symbols);
        event Action<VmNodeId, ScreenUpdate> ScreenUpdated;
        ScreenData GetScreen(VmNodeId nodeId);
    }
}

