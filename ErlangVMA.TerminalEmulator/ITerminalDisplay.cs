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
}
