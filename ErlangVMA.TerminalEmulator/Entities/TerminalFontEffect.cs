using System;

namespace ErlangVMA.TerminalEmulation
{
	[Flags]
	public enum TerminalFontEffect : byte
	{
		None = 0,
		Bold = 1,
		Italic = 2,
		Underlined = 4,
		DoubleUnderlined = 8
	}
}

