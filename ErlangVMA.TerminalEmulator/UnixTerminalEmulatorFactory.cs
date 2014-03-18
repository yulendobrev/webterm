using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ErlangVMA.TerminalEmulation
{
    public class UnixTerminalEmulatorFactory : ITerminalEmulatorFactory
    {
        private string executablePath;
        private string[] arguments;

        public UnixTerminalEmulatorFactory(string executablePath, string[] arguments)
        {
            this.executablePath = executablePath;
            this.arguments = arguments;
        }

        public TerminalEmulator CreateTerminalEmulator()
        {
            var terminalScreen = new TerminalScreen();
            var terminalStreamDecoder = new TerminalStreamDecoder(terminalScreen);
            var pseudoTerminal = new UnixPseudoTerminal();

            var terminalEmulator = new TerminalEmulator(executablePath, arguments, terminalStreamDecoder, terminalScreen, pseudoTerminal);

            return terminalEmulator;
        }
    }
}
