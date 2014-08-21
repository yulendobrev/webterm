using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ErlangVMA.TerminalEmulation
{
    [JsonObject]
    public class ScreenData : ScreenDisplayData
    {
        [JsonProperty("c")]
        public Point CursorPosition { get; set; }
    }
}
