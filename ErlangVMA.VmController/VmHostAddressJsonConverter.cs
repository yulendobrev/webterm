using System;
using Newtonsoft.Json;
using System.Net;

namespace ErlangVMA.VmController
{
	public class VmHostAddressJsonConverter : JsonConverter
	{
		public VmHostAddressJsonConverter()
		{
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var host = value as VmHostAddress;

			if (host != null)
			{
				var ipBytes = host.Ip.GetAddressBytes();
				serializer.Serialize(writer, ipBytes);
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var bytes = serializer.Deserialize<byte[]>(reader);
			var host = new VmHostAddress(new IPAddress(bytes));

			return host;
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(VmHostAddress);
		}
	}
}

