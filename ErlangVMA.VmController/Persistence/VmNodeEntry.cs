using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErlangVMA.VmController.Persistence
{
    public class VmNodeEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        [Required]
        public string User { get; set; }

        [Required]
        public string HostMachine { get; set; }

        public int VmNodeId { get; set; }

        public DateTime StartedOn { get; set; }
    }
}
