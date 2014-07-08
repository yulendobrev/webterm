using ErlangVMA.TerminalEmulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace ErlangVMA.VmController
{
    [ServiceContract(Namespace = "http://erlangvma.org/services", CallbackContract = typeof(IVmNodeEventListener))]
    public interface IVmNodeManagerService
    {
        [OperationContract]
        VmNodeId StartNewNode();

        [OperationContract]
        bool IsNodeAlive(VmNodeId address);

        [OperationContract]
        void ShutdownNode(VmNodeId address);

        [OperationContract]
        void SendInput(VmNodeId address, IEnumerable<byte> symbols);

        [OperationContract]
        ScreenData GetScreen(VmNodeId nodeId);
    }
}
