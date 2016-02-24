using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ErlangVMA.Web.Configuration
{
    public class ErlangExecutionEnvironmentSection : ConfigurationSection
    {
        private const string MachinesPropertyName = "machines";

        [ConfigurationProperty(MachinesPropertyName)]
        public ErlangExecutionMachinesCollectionElement ErlangExecutionMachines
        {
            get { return (ErlangExecutionMachinesCollectionElement)this[MachinesPropertyName]; }
            set { this[MachinesPropertyName] = value; }
        }
    }
}