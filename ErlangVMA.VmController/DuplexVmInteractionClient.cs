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
        private readonly Action<VmNodeId, ScreenData> onScreenUpdate;
        private readonly IPAddress address;
        private readonly int port;

        private readonly TcpClient client;
        private readonly BlockingCollection<Tuple<VmNodeId, IEnumerable<byte>>> sendQueue;

        public DuplexVmInteractionClient(Action<VmNodeId, ScreenData> onScreenUpdate, IPAddress address, int port)
        {
            this.onScreenUpdate = onScreenUpdate ?? new Action<VmNodeId, ScreenData>((nodeId, screenData) => { });
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
                        var screenUpdate = new ScreenData();

                        screenUpdate.X = streamReader.ReadInt32();
                        screenUpdate.Y = streamReader.ReadInt32();
                        screenUpdate.Width = streamReader.ReadInt32();
                        screenUpdate.Height = streamReader.ReadInt32();

                        screenUpdate.CursorPosition = new Point();
                        screenUpdate.CursorPosition.Row = streamReader.ReadInt32();
                        screenUpdate.CursorPosition.Column = streamReader.ReadInt32();

                        screenUpdate.Data = new TerminalScreenCharacter[streamReader.ReadInt32()];
                        for (int i = 0; i < screenUpdate.Data.Length; ++i)
                        {
                            var character = new TerminalScreenCharacter();
                            character.Character = streamReader.ReadChar();

                            character.Rendition = new ScreenCharacterRendition();
                            character.Rendition.Foreground = (TerminalColor)streamReader.ReadByte();
                            character.Rendition.Background = (TerminalColor)streamReader.ReadByte();

                            screenUpdate.Data[i] = character;
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
