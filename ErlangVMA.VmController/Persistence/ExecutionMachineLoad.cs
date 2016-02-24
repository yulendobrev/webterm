using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErlangVMA.VmController.Persistence
{
    public class ExecutionMachineLoad
    {
        [Key]
        [Required]
        public string Address { get; set; }

        public int VirtualMachineCount { get; set; }
    }
}
