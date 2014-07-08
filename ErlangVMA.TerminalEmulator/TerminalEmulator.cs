using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace ErlangVMA.TerminalEmulation
{
    public class TerminalEmulator
    {
        private ITerminalStreamDecoder terminalStreamDecoder;
        private ITerminalDisplay terminalDisplay;
        private IPseudoTerminal pseudoTerminal;

        private ReaderWriterLockSlim processingLock;
        private Stream inputStream;
        private int id;

        public TerminalEmulator(string executablePath, ITerminalStreamDecoder terminalStreamDecoder, ITerminalDisplay terminalDisplay, IPseudoTerminal pseudoTerminal)
            : this(executablePath, new string[0], terminalStreamDecoder, terminalDisplay, pseudoTerminal)
        {
        }

        public TerminalEmulator(string executablePath, string[] arguments, ITerminalStreamDecoder terminalStreamDecoder, ITerminalDisplay terminalDisplay, IPseudoTerminal pseudoTerminal)
        {
            this.terminalStreamDecoder = terminalStreamDecoder;
            this.terminalDisplay = terminalDisplay;
            this.pseudoTerminal = pseudoTerminal;

            this.processingLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

            terminalDisplay.ScreenUpdated += OnScreenUpdated;
            var streams = pseudoTerminal.CreatePseudoTerminal(executablePath, arguments);

            this.id = streams.ProcessId;
            this.inputStream = streams.InputStream;
            BeginAsyncOutputRead(streams.OutputStream);
        }

        public int Id
        {
            get { return id; }
        }

        public bool IsAlive
        {
            get
            {
                try
                {
                    return !Process.GetProcessById(id).HasExited;
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }
        }
        
        public event Action<ScreenData> ScreenUpdated;

        public void Input(IEnumerable<byte> symbols)
        {
            processingLock.EnterWriteLock();
            try
            {
                var inputBytes = symbols.ToArray();

                inputStream.Write(inputBytes, 0, inputBytes.Length);
                inputStream.Flush();
            }
            catch (IOException)
            {
            }
            finally
            {
                processingLock.ExitWriteLock();
            }
        }

        public ScreenData GetScreen()
        {
            processingLock.EnterReadLock();
            try
            {
                var screen = terminalDisplay.GetWholeScreen();
                return screen;
            }
            finally
            {
                processingLock.ExitReadLock();
            }
        }

        public void Shutdown()
        {
            try
            {
                var process = Process.GetProcessById(id);
                try
                {
                    process.Kill();
                }
                catch (InvalidOperationException)
                { }
            }
            catch (ArgumentException)
            { }
        }

        private void OnScreenUpdated(ScreenData screenData)
        {
            RaiseScreenUpdated(screenData);
        }

        private void BeginAsyncOutputRead(Stream output)
        {
            byte[] buffer = new byte[4096];

            DoAsyncOutputRead(buffer, output);
        }

        private void DoAsyncOutputRead(byte[] buffer, Stream outputStream)
        {
            if (outputStream == null)
                return;

            try
            {
                outputStream.BeginRead(buffer, 0, buffer.Length, ar =>
                {
                    try
                    {
                        int bytesRead = outputStream.EndRead(ar);
                        terminalStreamDecoder.ProcessInput(buffer.Take(bytesRead));

                        DoAsyncOutputRead(buffer, outputStream);
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }, null);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void RaiseScreenUpdated(ScreenData screenData)
        {
            var handler = ScreenUpdated;
            if (handler != null)
            {
                handler(screenData);
            }
        }
    }
}
