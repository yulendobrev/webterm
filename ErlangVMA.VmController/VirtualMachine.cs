using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErlangVMA.VmController
{
    public class VirtualMachine
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public VmNodeAddress NodeAddress { get; set; }

        public DateTime StartedOn { get; set; }

        public bool IsActive { get; set; }
    }
}
