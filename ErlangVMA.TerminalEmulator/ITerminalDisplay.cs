using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ErlangVMA.TerminalEmulation
{
	public interface ITerminalDisplay
	{
		ScreenData GetWholeScreen();
		event Action<ScreenData> ScreenUpdated;
	}

	public class ScreenData
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public char[] Data { get; set; }
		public Point CursorPosition { get; set; }
	}
}
