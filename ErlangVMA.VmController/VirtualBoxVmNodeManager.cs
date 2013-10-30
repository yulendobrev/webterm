using System;
using System.Diagnostics;

namespace ErlangVMA.VmController
{
	public class VirtualBoxVmNodeManager
	{
		private const string VBoxManage = "VBoxManage";

		private string username;
		private bool started;

		public VirtualBoxVmNodeManager()
		{
		}

		public string VirtualMachineImageName { get; set; }

		public void Initialize(string username)
		{
			this.username = username;
		}

		public void StartNode()
		{
			string cloneName = CloneVirtualMachineImage();
			SetupVirtualMachine(cloneName);

			StartVirtualMachine(cloneName);

			//WaitForConnection();
		}

		private bool DoesVirtualMachineExist(string name)
		{
			return ExecuteShellCommand(VBoxManage, string.Format("showvminfo \"{0}\"", name)) != 0;
		}

		private string CloneVirtualMachineImage()
		{
			string virtualMachineName = string.Format("{0} - {1}", VirtualMachineImageName, username);

			ExecuteShellCommand(VBoxManage, string.Format("clonevm \"{0}\" --name \"{1}\" --register", VirtualMachineImageName, virtualMachineName));

			return virtualMachineName;
		}

		private void SetupVirtualMachine(string virtualMachineName)
		{
		}

		private void StartVirtualMachine(string virtualMachineName)
		{
			ExecuteShellCommand(VBoxManage, string.Format("startvm \"{0}\" --type headless", virtualMachineName));
			started = true;
		}

		private int ExecuteShellCommand(string command, string arguments)
		{
			var process = new Process();
			process.StartInfo.UseShellExecute = true;
			process.StartInfo.FileName = command;
			process.StartInfo.Arguments = arguments;

			process.Start();
			process.WaitForExit();

			return process.ExitCode;
		}

		public void SendCommandLine(string commandLine)
		{
			if (!started)
				StartNode();

			ExecuteShellCommand(VBoxManage, "list vms");
		}

		public event Action<string> OnOutput;

		public void ShutdownNode()
		{
		}
	}
}
