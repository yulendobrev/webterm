using System;
using System.Collections.Generic;

namespace ErlangVMA.TerminalEmulation
{
	public interface ITerminalStreamDecoder
	{
		void ProcessInput(IEnumerable<byte> bytes);
	}
}

