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

		private Stream inputStream;

		private ReaderWriterLockSlim processingLock;

		private FileStream logFile;

		public static TerminalEmulator Create(string executablePath)
		{
			var terminalScreen = new TerminalScreen();
			var terminalStreamDecoder = new TerminalStreamDecoder(terminalScreen);
			var pseudoTerminal = new UnixPseudoTerminal();

			return new TerminalEmulator(executablePath, terminalStreamDecoder, terminalScreen, pseudoTerminal);
		}

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

			this.logFile = new FileStream("output", FileMode.Create | FileMode.Append, FileAccess.Write);

			terminalDisplay.ScreenUpdated += OnScreenUpdated;
			var streams = pseudoTerminal.CreatePseudoTerminal(executablePath, arguments);

			inputStream = streams.InputStream;

			BeginAsyncOutputRead(streams.OutputStream);
		}

		public int Id
		{
			get { return 0; }
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

		private void OnScreenUpdated(ScreenData screenData)
		{
			RaiseScreenUpdated(screenData);
		}

		private string CommandLineEscape(string username)
		{
			string escapedUsername = new string (
				(from c in username
				 select Char.IsLetterOrDigit (c) || Char.IsWhiteSpace (c) ? c : '_').ToArray()
			);

			return escapedUsername;
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
				outputStream.BeginRead(buffer, 0, buffer.Length, ar => {
					try
					{
						int bytesRead = outputStream.EndRead(ar);
						terminalStreamDecoder.ProcessInput(buffer.Take(bytesRead));

						logFile.Write(buffer, 0, bytesRead);
						logFile.Flush();

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
