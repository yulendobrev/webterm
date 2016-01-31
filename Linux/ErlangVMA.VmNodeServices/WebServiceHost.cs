using System;
using System.Net;
using System.ServiceModel;
using ErlangVMA.VmController;
using ErlangVMA.TerminalEmulation;

namespace ErlangVMA.VmNodeServices
{
	public class WebServiceHost
	{
		public static void Main(string[] args)
		{
			var vmNodeManager = new LocalShellVmNodeManager(new UnixTerminalEmulatorFactory("/usr/bin/erl"));
			var vmNodeManagerService = new VirtualVmNodeService(vmNodeManager);

			var host = new ServiceHost(vmNodeManagerService);
			host.Open();

			var duplexServer = new DuplexVmInteractionServer(vmNodeManager, IPAddress.Parse("192.168.122.1"), 4300);
			duplexServer.Start();

			Console.WriteLine("Press any key to stop server ...");
			Console.ReadKey();
			try
			{
				host.Close(TimeSpan.FromSeconds(10.0));
			}
			catch
			{
				host.Abort();
			}
		}
	}
}
