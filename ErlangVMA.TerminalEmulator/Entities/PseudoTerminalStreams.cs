using System;
using System.IO;

namespace ErlangVMA.TerminalEmulation
{
	public class PseudoTerminalStreams
	{
		public PseudoTerminalStreams()
		{
		}

		public Stream InputStream { get; set; }
		public Stream OutputStream { get; set; }
	}
}
