using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ErlangVMA.VmController
{
    public class ExecutionEngineMachine
    {
        public string IpAddress { get; set; }

        public int DuplexInteractionServerPort { get; set; }

        public string VirtualMachineServiceEndpointConfiguration { get; set; }
    }
}
