using System;
using System.Collections.Generic;
using ErlangVMA.TerminalEmulation;

namespace ErlangVMA.VmController
{
    public interface IVmBroker
    {
        IEnumerable<VirtualMachine> GetVirtualMachines(VmUser user);
        int StartNewNode(VmUser user, VirtualMachineStartOptions startOptions);
        int StartNewNode(VmUser user, VmHostAddress host, VirtualMachineStartOptions startOptions);
        void ShutdownNode(VmUser user, int virtualMachineId);
        void SendInput(VmUser user, int virtualMachineId, IEnumerable<byte> symbols);
        ScreenData GetScreen(VmUser user, int virtualMachineId);

        event Action<VmUser, int, ScreenUpdate> ScreenUpdated;
    }
}

