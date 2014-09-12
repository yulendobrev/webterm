using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErlangVMA.VmController
{
    public class VirtualMachineStartOptions
    {
        [Required(ErrorMessage = "Please provide a name for the virtual machine")]
        public string Name { get; set; }
    }
}
