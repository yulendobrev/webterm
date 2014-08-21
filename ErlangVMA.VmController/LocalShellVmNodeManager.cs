using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ErlangVMA.TerminalEmulation;

namespace ErlangVMA.VmController
{
    public class LocalShellVmNodeManager : IVmNodeManager
    {
        private readonly IDictionary<VmNodeId, TerminalEmulator> terminalEmulators;
        private readonly ITerminalEmulatorFactory terminalEmulatorFactory;

        public LocalShellVmNodeManager(ITerminalEmulatorFactory terminalEmulatorFactory)
        {
            this.terminalEmulatorFactory = terminalEmulatorFactory;
            this.terminalEmulators = new Dictionary<VmNodeId, TerminalEmulator>();
        }

        public event Action<VmNodeId, ScreenUpdate> ScreenUpdated;

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
            var terminalEmulator = GetTerminalEmulator(address);
            return terminalEmulator.IsAlive;
        }

        public void ShutdownNode(VmNodeId address)
        {
            var terminalEmulator = GetTerminalEmulator(address);
            terminalEmulator.Shutdown();
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
            TerminalEmulator terminalEmulator;
            if (!terminalEmulators.TryGetValue(address, out terminalEmulator))
            {
                throw new InvalidOperationException(string.Format("Node with id {0} not found", address));
            }

            return terminalEmulator;
        }

        private void RaiseScreenUpdated(VmNodeId nodeId, ScreenData screenData)
        {
            var screenUpdatedHandler = ScreenUpdated;
            if (screenUpdatedHandler != null)
            {
                screenUpdatedHandler(nodeId, new ScreenUpdate
                {
                    CursorPosition = screenData.CursorPosition,
                    DisplayUpdates = new List<ScreenDisplayData> { screenData }
                });
            }
        }
    }
}

