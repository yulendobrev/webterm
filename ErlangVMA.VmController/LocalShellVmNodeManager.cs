using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ErlangVMA.TerminalEmulation;
using System.Collections.Generic;

namespace ErlangVMA.VmController
{
    public class LocalShellVmNodeManager : IVmNodeManager
    {
        private Dictionary<VmNodeId, TerminalEmulator> terminalEmulators;
        private ITerminalEmulatorFactory terminalEmulatorFactory;

        public LocalShellVmNodeManager(ITerminalEmulatorFactory terminalEmulatorFactory)
        {
            this.terminalEmulatorFactory = terminalEmulatorFactory;
            this.terminalEmulators = new Dictionary<VmNodeId, TerminalEmulator>();
        }

        public event Action<VmNodeId, ScreenData> ScreenUpdated;

        public VmNodeId StartNewNode()
        {
            var terminalEmulator = terminalEmulatorFactory.CreateTerminalEmulator();
            var nodeId = new VmNodeId(terminalEmulator.Id);

            terminalEmulator.ScreenUpdated += s => RaiseScreenUpdated(nodeId, s);

            terminalEmulators.Add(nodeId, terminalEmulator);

            return nodeId;
        }

        public bool IsNodeAlive(VmNodeId address)
        {
            return true;
        }

        public void ShutdownNode(VmNodeId address)
        {
        }

        public void SendInput(VmNodeId address, IEnumerable<byte> symbols)
        {
            var terminalEmulator = GetTerminalEmulator(address);
            terminalEmulator.Input(symbols);
        }

        public ScreenData GetScreen(VmNodeId address)
        {
            var terminalEmulator = GetTerminalEmulator(address);
            var screen = terminalEmulator.GetScreen();

            return screen;
        }

        private TerminalEmulator GetTerminalEmulator(VmNodeId address)
        {
            return terminalEmulators[address];
        }

        private void RaiseScreenUpdated(VmNodeId nodeId, ScreenData screenData)
        {
            var screenUpdatedHandler = ScreenUpdated;
            if (screenUpdatedHandler != null)
            {
                screenUpdatedHandler(nodeId, screenData);
            }
        }
    }
}

