using System;

namespace ErlangVMA
{
	public class VirtualMachineConsole : IVirtualMachineConsole
	{
		public VirtualMachineConsole ()
		{
		}

		public string ExecuteLine (string input)
		{
			return input;
		}
	}
}

