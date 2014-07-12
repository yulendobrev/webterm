using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace ErlangVMA.VmController.Persistence
{
    public class VmNodesDbModel : DbContext
    {
        // Your context has been configured to use a 'VmNodesDbModel' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'ErlangVMA.VmController.VmNodesDbModel' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'VmNodesDbModel' 
        // connection string in the application configuration file.
        public VmNodesDbModel()
            : base("name=VmNodesDbModel")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<VmNodeDbEntry> ActiveVmNodes { get; set; }
    }
}