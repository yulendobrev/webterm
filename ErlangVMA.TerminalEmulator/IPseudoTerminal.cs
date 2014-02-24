using System;

namespace ErlangVMA.TerminalEmulation
{
	public interface IPseudoTerminal
	{
		PseudoTerminalStreams CreatePseudoTerminal(string executablePath, string[] arguments);
	}
}
