using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ErlangVMA.TerminalEmulation;

namespace ErlangVMA.VmController
{
    public class Screen
    {
        public Point CursorPosition { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public TerminalScreenCharacter[] Data { get; set; }
    }
}
