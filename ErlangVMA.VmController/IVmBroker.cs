using System;
using System.Collections.Generic;
using ErlangVMA.TerminalEmulation;

namespace ErlangVMA.VmController
{
    public interface IVmBroker
    {
        IEnumerable<VirtualMachine> GetVirtualMachines(VmUser user);
        VmNodeAddress StartNewNode(VmUser user, VirtualMachineStartOptions startOptions);
        VmNodeAddress StartNewNode(VmUser user, VmHostAddress host, VirtualMachineStartOptions startOptions);
        void ShutdownNode(VmUser user, VmNodeAddress address);
        void SendInput(VmUser user, VmNodeAddress address, IEnumerable<byte> symbols);
        event Action<VmUser, VmNodeAddress, ScreenData> ScreenUpdated;
        ScreenData GetScreen(VmUser user, VmNodeAddress address);
    }
}

