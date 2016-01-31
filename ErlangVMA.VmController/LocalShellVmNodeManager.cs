using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            this.terminalEmulators = new ConcurrentDictionary<VmNodeId, TerminalEmulator>();
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
			return terminalEmulator != null && terminalEmulator.IsAlive;
        }

        public void ShutdownNode(VmNodeId address)
        {
            var terminalEmulator = GetTerminalEmulator(address);
			if (terminalEmulator != null)
			{
				terminalEmulator.Shutdown();
			}
        }

        public void SendInput(VmNodeId address, IEnumerable<byte> symbols)
        {
            var terminalEmulator = GetTerminalEmulator(address);
			if (terminalEmulator != null)
			{
				terminalEmulator.Input(symbols);
			}
        }

        public ScreenData GetScreen(VmNodeId address)
        {
            var terminalEmulator = GetTerminalEmulator(address);
			if (terminalEmulator != null)
			{
				var screen = terminalEmulator.GetScreen();
				return screen;
			}

			return new ScreenData { CursorPosition = new Point(), Data = new TerminalScreenCharacter[0] };
        }

        private TerminalEmulator GetTerminalEmulator(VmNodeId address)
        {
            TerminalEmulator terminalEmulator;
			terminalEmulators.TryGetValue(address, out terminalEmulator);

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
