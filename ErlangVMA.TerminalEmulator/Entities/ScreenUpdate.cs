using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ErlangVMA.TerminalEmulation
{
    [JsonObject]
    public class ScreenUpdate
    {
        public ScreenUpdate()
        {
            this.DisplayUpdates = new List<ScreenDisplayData>();
        }

        [JsonProperty("u")]
        public List<ScreenDisplayData> DisplayUpdates { get; set; }

        [JsonProperty("c")]
        public Point CursorPosition { get; set; }
    }
}
