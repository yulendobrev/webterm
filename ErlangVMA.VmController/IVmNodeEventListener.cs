using ErlangVMA.TerminalEmulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace ErlangVMA.VmController
{
    [ServiceContract]
    public interface IVmNodeEventListener
    {
        [OperationContract(IsOneWay = true)]
        void ScreenUpdated(VmNodeId nodeId, ScreenData screenData);
    }
}
