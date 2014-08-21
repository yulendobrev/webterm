using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ErlangVMA.TerminalEmulation
{
    [JsonObject]
    public class ScreenDisplayData
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
    }
}
