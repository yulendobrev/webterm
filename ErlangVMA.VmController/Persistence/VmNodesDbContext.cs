using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace ErlangVMA.VmController.Persistence
{
    public class VmNodesDbContext : DbContext
    {
        // Your context has been configured to use a 'VmNodesDbContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'ErlangVMA.VmController.VmNodesDbContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'VmNodesDbContext' 
        // connection string in the application configuration file.
        public VmNodesDbContext()
            : base("name=VmNodesDbContext")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<VmNodeEntry> ActiveVmNodes { get; set; }

        public virtual DbSet<ExecutionMachineLoad> ExecutionMachineLoads { get; set; }
    }
}