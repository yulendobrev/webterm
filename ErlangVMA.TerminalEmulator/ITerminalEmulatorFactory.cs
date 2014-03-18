using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ErlangVMA.TerminalEmulation
{
    public interface ITerminalEmulatorFactory
    {
        TerminalEmulator CreateTerminalEmulator();
    }
}
