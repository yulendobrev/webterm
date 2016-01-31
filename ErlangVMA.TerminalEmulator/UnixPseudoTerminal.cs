using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace ErlangVMA.TerminalEmulation
{
    public class UnixPseudoTerminal : IPseudoTerminal
    {
        public UnixPseudoTerminal()
        {
        }

        public PseudoTerminalStreams CreatePseudoTerminal(string executablePath, string[] arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(
                                             "../../../ErlangVMA.TerminalEmulation.PseudoTerminalWrapper/bin/Debug/ErlangVMA.TerminalEmulation",
                                             string.Format("{0} {1}", executablePath, string.Join(" ", arguments)));

            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;

            Process process = new Process();
            process.StartInfo = startInfo;

            process.Start();

            var streams = new PseudoTerminalStreams();
            streams.ProcessId = process.Id;
            streams.InputStream = process.StandardInput.BaseStream;
            streams.OutputStream = process.StandardOutput.BaseStream;

            return streams;
        }
    }
}
