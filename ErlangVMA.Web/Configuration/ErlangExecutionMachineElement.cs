using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ErlangVMA.Web.Configuration
{
    public class ErlangExecutionMachineElement : ConfigurationElement
    {
        private const string AddressPropertyName = "address";
        private const string DuplexInteractionServerPortPropertyName = "duplexInteractionServerPort";
        private const string VirtualMachineServiceEndpointConfigurationPropertyName = "virtualMachineServiceEndpointConfiguration";

        [ConfigurationProperty(AddressPropertyName)]
        public string Address
        {
            get { return (string)this[AddressPropertyName]; }
            set { this[AddressPropertyName] = value; }
        }

        [ConfigurationProperty(DuplexInteractionServerPortPropertyName)]
        public int DuplexInteractionServerPort
        {
            get { return (int)this[DuplexInteractionServerPortPropertyName]; }
            set { this[DuplexInteractionServerPortPropertyName] = value; }
        }

        [ConfigurationProperty(VirtualMachineServiceEndpointConfigurationPropertyName)]
        public string VirtualMachineServiceEndpointConfiguration
        {
            get { return (string)this[VirtualMachineServiceEndpointConfigurationPropertyName]; }
            set { this[VirtualMachineServiceEndpointConfigurationPropertyName] = value; }
        }
    }
}
