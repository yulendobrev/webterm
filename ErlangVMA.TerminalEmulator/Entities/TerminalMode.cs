using System;
using System.Collections.Generic;

namespace ErlangVMA.TerminalEmulation
{
	public enum TerminalMode
	{
		ShowCursor,
      	HideCursor,
      	LineFeed,
      	NewLine,
      	CursorKeyToCursor,
      	CursorKeyToApplication,
      	ANSI,
      	VT52,
      	Columns80,
      	Columns132,
      	JumpScrolling,
      	SmoothScrolling,
      	NormalVideo,
      	ReverseVideo,
      	OriginIsAbsolute,
      	OriginIsRelative,
      	LineWrap,
      	DisableLineWrap,
      	AutoRepeat,
      	DisableAutoRepeat,
      	Interlacing,
      	DisableInterlacing,
      	NumericKeypad,
      	AlternateKeypad,
	}
}

