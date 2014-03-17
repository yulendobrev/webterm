using System;
using System.ServiceModel;

namespace ErlangVMA
{
	[ServiceContract]
	public interface IVirtualMachineConsole
	{
		[OperationContract]
		string ExecuteLine(string input);
	}
}
