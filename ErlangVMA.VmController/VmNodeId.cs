using System;
using Newtonsoft.Json;

namespace ErlangVMA.VmController
{
    //[JsonConverter(typeof(VmNodeIdJsonConverter))]
    public class VmNodeId
    {
        private int id;

        public VmNodeId()
        {
        }

        public VmNodeId(int id)
        {
            this.id = id;
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public override bool Equals(object obj)
        {
            var nodeId = obj as VmNodeId;
            return (object)nodeId != null && id == nodeId.id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static bool operator==(VmNodeId nodeId, VmNodeId otherNodeId)
        {
            return (object)nodeId == null && (object)otherNodeId == null || (object)nodeId != null && nodeId.Equals(otherNodeId);
        }

        public static bool operator!=(VmNodeId nodeId, VmNodeId otherNodeId)
        {
            return !(nodeId == otherNodeId);
        }

        public override string ToString()
        {
            return id.ToString();
        }
    }
}
