using System;
using System.Net;
using Newtonsoft.Json;

namespace ErlangVMA.VmController
{
    [JsonConverter(typeof(VmHostAddressJsonConverter))]
    public class VmHostAddress
    {
        private IPAddress ip;

        private static VmHostAddress local = new VmHostAddress(IPAddress.Loopback);

        public VmHostAddress() : this(null)
        {
        }

        public VmHostAddress(IPAddress ip)
        {
            this.ip = ip;
        }

        public IPAddress Ip
        {
            get { return ip; }
            set { ip = value; }
        }

        public static VmHostAddress Local
        {
            get { return local; }
        }

        public override bool Equals(object obj)
        {
            var hostAddress = obj as VmHostAddress;
            return (object)hostAddress != null;// && Ip == hostAddress.Ip;
        }

        public override int GetHashCode()
        {
            return Ip != null ? Ip.GetHashCode() : 0;
        }

        public static bool operator==(VmHostAddress hostAddress, VmHostAddress otherHostAddress)
        {
            return ((object)hostAddress == null && (object)otherHostAddress == null) || ((object)hostAddress != null && hostAddress.Equals(otherHostAddress));
        }

        public static bool operator!=(VmHostAddress hostAddress, VmHostAddress otherHostAddress)
        {
            return !(hostAddress == otherHostAddress);
        }

        public override string ToString()
        {
            return Ip != null ? Ip.ToString() : "<unknown address>";
        }
    }
}

