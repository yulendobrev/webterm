using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using ErlangVMA.VmController;

namespace ErlangVMA.VmNodeServices
{
	public class DuplexVmInteractionServer
	{
		private readonly IVmNodeManager vmNodeManager;
		private readonly TcpListener listener;

		public DuplexVmInteractionServer(IVmNodeManager vmNodeManager, IPAddress address, int port)
		{
			this.vmNodeManager = vmNodeManager;
			listener = new TcpListener(address, port);
		}

		public void Start()
		{
			listener.Start();
			listener.BeginAcceptTcpClient(OnClientConnected, null);
		}

		private void OnClientConnected(IAsyncResult ar)
		{
			var client = listener.EndAcceptTcpClient(ar);
			var stream = client.GetStream();

			Debug.WriteLine("Client {0} connected", client.Client.RemoteEndPoint);

			var worker = new DuplexVmInteractionWorker(stream, vmNodeManager);
			worker.InteractAsync().ContinueWith(t =>
			{
				try
				{
					if (client.Connected)
					{
						client.Close();
					}
				}
				catch
				{
				}
				Debug.WriteLine("Client {0} disconnected", client.Client.RemoteEndPoint);
			});

			listener.BeginAcceptTcpClient(OnClientConnected, null);
		}
	}
}
