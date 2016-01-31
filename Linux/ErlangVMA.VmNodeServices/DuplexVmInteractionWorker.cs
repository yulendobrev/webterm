using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using ErlangVMA.TerminalEmulation;
using ErlangVMA.VmController;

namespace ErlangVMA.VmNodeServices
{
	internal class DuplexVmInteractionWorker
	{
		private readonly NetworkStream stream;
		private readonly IVmNodeManager vmNodeManager;

		private BlockingCollection<Tuple<VmNodeId, ScreenUpdate>> sendQueue;
		private bool isSending;
		private ConcurrentDictionary<VmNodeId, ScreenUpdateRateLimiter> rateLimiters;

		public DuplexVmInteractionWorker(NetworkStream stream, IVmNodeManager vmNodeManager)
		{
			this.vmNodeManager = vmNodeManager;
			this.stream = stream;

			sendQueue = new BlockingCollection<Tuple<VmNodeId, ScreenUpdate>>();
			rateLimiters = new ConcurrentDictionary<VmNodeId, ScreenUpdateRateLimiter>();

			vmNodeManager.ScreenUpdated += OnScreenUpdated;
		}

		public Task InteractAsync()
		{
			Debug.WriteLine("Interaction with client started");
			isSending = true;
			return Task.WhenAny(ReadInputAsync(), SendScreenUpdatesAsync())
				.ContinueWith(t => Debug.WriteLine("Interaction with client ended"));
		}

		private Task ReadInputAsync()
		{
			return Task.Run(() =>
			{
				var streamReader = new BinaryReader(stream);
				try
				{
					Debug.WriteLine("Started reading input");
					while (true)
					{
						var nodeId = new VmNodeId(streamReader.ReadInt32());
						var bytes = streamReader.ReadBytes(streamReader.ReadInt32());

						Debug.WriteLine("Sending input to node manager");
						vmNodeManager.SendInput(nodeId, bytes);
						Debug.WriteLine("Input sent to node manager");
					}
				}
				catch (EndOfStreamException)
				{
					isSending = false;
					sendQueue.CompleteAdding();
				}
				Debug.WriteLine("Ended reading input");
			});
		}

		private async Task SendScreenUpdatesAsync()
		{
			Debug.WriteLine("Started sending updates");
			while (isSending)
			{
				Tuple<VmNodeId, ScreenUpdate> item;
				try
				{
					item = sendQueue.Take();
				}
				catch (InvalidOperationException)
				{
					break;
				}
				await SendScreenUpdatesAsync(item.Item1, item.Item2);
			}
			Debug.WriteLine("Ended sending updates");
		}

		private void OnScreenUpdated(VmNodeId nodeId, ScreenUpdate screenUpdate)
		{
			var rateLimiter = rateLimiters.GetOrAdd(nodeId, id => new ScreenUpdateRateLimiter(data => QueueScreenUpdateForSending(id, data)));

			foreach (var displayData in screenUpdate.DisplayUpdates)
			{
				rateLimiter.AddScreenUpdate(new ScreenData
				{
					CursorPosition = screenUpdate.CursorPosition,
					X = displayData.X,
					Y = displayData.Y,
					Width = displayData.Width,
					Height = displayData.Height,
					Data = displayData.Data
				});
			}
		}

		private void QueueScreenUpdateForSending(VmNodeId nodeId, ScreenUpdate screenData)
		{
			if (isSending)
			{
				sendQueue.Add(Tuple.Create(nodeId, screenData));
			}
			else
			{
				vmNodeManager.ScreenUpdated -= OnScreenUpdated;
			}
		}

		private Task SendScreenUpdatesAsync(VmNodeId nodeId, ScreenUpdate data)
		{
			return Task.Run(() =>
			{
				//Debug.WriteLine("Sending screen update");
				var streamWriter = new BinaryWriter(stream);
				try
				{
					streamWriter.Write(nodeId.Id);
					streamWriter.Write(data.CursorPosition.Row);
					streamWriter.Write(data.CursorPosition.Column);

					streamWriter.Write(data.DisplayUpdates.Count);
					foreach (var displayUpdate in data.DisplayUpdates)
					{
						streamWriter.Write(displayUpdate.X);
						streamWriter.Write(displayUpdate.Y);
						streamWriter.Write(displayUpdate.Width);
						streamWriter.Write(displayUpdate.Height);

						streamWriter.Write(displayUpdate.Data.Length);
						foreach (var c in displayUpdate.Data)
						{
							streamWriter.Write(c.Character);
							streamWriter.Write((byte)c.Rendition.Foreground);
							streamWriter.Write((byte)c.Rendition.Background);
						}
					}
					streamWriter.Flush();
					//Debug.WriteLine("Screen update sent");
				}
				catch (IOException)
				{
					vmNodeManager.ScreenUpdated -= OnScreenUpdated;
					isSending = false;
				}
			});
		}
	}
}
