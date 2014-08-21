using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ErlangVMA.TerminalEmulation;

namespace ErlangVMA.VmController
{
    public class DuplexVmInteractionClient
    {
        private readonly Action<VmNodeId, ScreenUpdate> onScreenUpdate;
        private readonly IPAddress address;
        private readonly int port;

        private readonly TcpClient client;
        private readonly BlockingCollection<Tuple<VmNodeId, IEnumerable<byte>>> sendQueue;

        public DuplexVmInteractionClient(Action<VmNodeId, ScreenUpdate> onScreenUpdate, IPAddress address, int port)
        {
            this.onScreenUpdate = onScreenUpdate ?? new Action<VmNodeId, ScreenUpdate>((nodeId, screenData) => { });
            this.address = address;
            this.port = port;

            this.client = new TcpClient();
            this.sendQueue = new BlockingCollection<Tuple<VmNodeId, IEnumerable<byte>>>();
        }

        public Task InteractAsync()
        {
            return client.ConnectAsync(address, port)
                         .ContinueWith(t => Task.WhenAny(ReadScreenUpdatesAsync(), SendInputAsync())).Unwrap();
        }

        public void SendInput(VmNodeId nodeId, IEnumerable<byte> bytes)
        {
            sendQueue.Add(Tuple.Create(nodeId, bytes));
        }

        private Task ReadScreenUpdatesAsync()
        {
            return Task.Run(() =>
            {
                var streamReader = new BinaryReader(client.GetStream());
                try
                {
                    while (true)
                    {
                        var vmNodeId = new VmNodeId(streamReader.ReadInt32());
                        var screenUpdate = new ScreenUpdate();

                        screenUpdate.CursorPosition = new Point();
                        screenUpdate.CursorPosition.Row = streamReader.ReadInt32();
                        screenUpdate.CursorPosition.Column = streamReader.ReadInt32();

                        int displayUpdateCount = streamReader.ReadInt32();
                        screenUpdate.DisplayUpdates = new List<ScreenDisplayData>(displayUpdateCount);

                        for (int i = 0; i < displayUpdateCount; ++i)
                        {
                            var displayData = new ScreenDisplayData();
                            screenUpdate.DisplayUpdates.Add(displayData);

                            displayData.X = streamReader.ReadInt32();
                            displayData.Y = streamReader.ReadInt32();
                            displayData.Width = streamReader.ReadInt32();
                            displayData.Height = streamReader.ReadInt32();

                            displayData.Data = new TerminalScreenCharacter[streamReader.ReadInt32()];
                            for (int j = 0; j < displayData.Data.Length; ++j)
                            {
                                var character = new TerminalScreenCharacter();
                                character.Character = streamReader.ReadChar();

                                character.Rendition = new ScreenCharacterRendition();
                                character.Rendition.Foreground = (TerminalColor)streamReader.ReadByte();
                                character.Rendition.Background = (TerminalColor)streamReader.ReadByte();

                                displayData.Data[j] = character;
                            }
                        }

                        onScreenUpdate(vmNodeId, screenUpdate);
                    }
                }
                catch (EndOfStreamException)
                {
                }
            });
        }

        private Task SendInputAsync()
        {
            return Task.Run(() =>
            {
                var streamWriter = new BinaryWriter(client.GetStream());
                while (true)
                {
                    var input = sendQueue.Take();

                    var symbols = input.Item2.ToArray();
                    try
                    {
                        streamWriter.Write(input.Item1.Id);
                        streamWriter.Write(symbols.Length);
                        streamWriter.Write(symbols);
                    }
                    catch (EndOfStreamException)
                    {
                        break;
                    }
                }
            });
        }
    }
}
