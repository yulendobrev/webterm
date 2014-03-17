using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ErlangVMA.TerminalEmulation
{
	public interface ITerminalDisplay
	{
		ScreenData GetWholeScreen();
		event Action<ScreenData> ScreenUpdated;
	}

	[JsonObject]
	public class ScreenData
	{
		[JsonProperty("x")]
		public int X { get; set; }

		[JsonProperty("y")]
		public int Y { get; set; }

		[JsonProperty("w")]
		public int Width { get; set; }

		[JsonProperty("h")]
		public int Height { get; set; }

		[JsonProperty("d")]
		public TerminalScreenCharacter[] Data { get; set; }

		[JsonProperty("c")]
		public Point CursorPosition { get; set; }
	}
}
