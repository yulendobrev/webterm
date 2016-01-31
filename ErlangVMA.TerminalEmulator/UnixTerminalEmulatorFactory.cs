namespace ErlangVMA.TerminalEmulation
{
    public class UnixTerminalEmulatorFactory : ITerminalEmulatorFactory
    {
        private readonly string executablePath;
        private readonly string[] arguments;

		public UnixTerminalEmulatorFactory(string executablePath) : this(executablePath, new string[0])
		{ }

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
