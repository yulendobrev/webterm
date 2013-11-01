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
		private Process process;
		private ITerminalStreamDecoder terminalStreamDecoder;
		private ITerminalDisplay terminalDisplay;
		private ReaderWriterLockSlim processingLock;

		public static TerminalEmulator Create(string executablePath)
		{
			var terminalScreen = new TerminalScreen();
			var terminalStreamDecoder = new TerminalStreamDecoder(terminalScreen);

			return new TerminalEmulator(executablePath, terminalStreamDecoder, terminalScreen);
		}

		public TerminalEmulator(string executablePath, ITerminalStreamDecoder terminalStreamDecoder, ITerminalDisplay terminalDisplay)
		{
			this.terminalStreamDecoder = terminalStreamDecoder;
			this.terminalDisplay = terminalDisplay;
			this.processingLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			terminalDisplay.ScreenUpdated += OnScreenUpdated;

			var startInfo = new ProcessStartInfo(executablePath);

//			if (!string.IsNullOrEmpty (username))
//				startInfo.Arguments = string.Format ("-sname \"{0}\"", CommandLineEscape (username));

			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardError = true;
			startInfo.RedirectStandardInput = true;
			startInfo.RedirectStandardOutput = true;

			process = new Process();
			process.StartInfo = startInfo;
			process.EnableRaisingEvents = true;

			process.Exited += HandleExited;

			process.Start();
			BeginAsyncOutputRead(process.StandardOutput);
			BeginAsyncOutputRead(process.StandardError);
		}

		public int Id
		{
			get { return process.Id; }
		}
		
		public event Action<ScreenData> ScreenUpdated;

		public void Input(IEnumerable<byte> symbols)
		{
			processingLock.EnterWriteLock();
			try
			{
				var inputStream = process.StandardInput.BaseStream;

				var inputBytes = symbols.ToArray();

				terminalStreamDecoder.ProcessInput(symbols);

				inputStream.Write(inputBytes, 0, inputBytes.Length);
				inputStream.Flush();
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

		private void BeginAsyncOutputRead(StreamReader outputReader)
		{
			byte[] buffer = new byte[4096];

			DoAsyncOutputRead(buffer, outputReader);
		}

		private void DoAsyncOutputRead(byte[] buffer, StreamReader outputReader)
		{
			var outputStream = outputReader.BaseStream;

			if (outputStream == null)
				return;

			outputStream.BeginRead(buffer, 0, buffer.Length, ar => {
				try
				{
					int bytesRead = outputStream.EndRead(ar);
					terminalStreamDecoder.ProcessInput(buffer.Take(bytesRead));

					DoAsyncOutputRead(buffer, outputReader);
				}
				catch (ObjectDisposedException)
				{
				}
			}, null);
		}

		private void HandleExited(object sender, EventArgs e)
		{
			process.Dispose();
			process = null;
		}

		private bool IsStarted()
		{
			return process != null && !process.HasExited;
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
