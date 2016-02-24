using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ErlangVMA.Web.Configuration
{
    [ConfigurationCollection(typeof(ErlangExecutionMachineElement), AddItemName = "machine")]
    public class ErlangExecutionMachinesCollectionElement : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ErlangExecutionMachineElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ErlangExecutionMachineElement)element).Address;
        }
    }
}
