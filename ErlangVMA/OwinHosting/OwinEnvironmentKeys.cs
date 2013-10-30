using System;

namespace ErlangVMA
{
	public static class OwinEnvironmentKeys
	{
		/// <summary>
		/// A Stream with the request body, if any. Stream.Null MAY be used as a placeholder if there is no request body. See Request Body.
		/// </summary>
		public const string Request = "owin.RequestBody";

		/// <summary>
		/// An IDictionary<string, string[]> of request headers.  See Headers.
		/// </summary>
		public const string RequestHeaders = "owin.RequestHeaders";

		/// <summary>
		/// A string containing the HTTP request method of the request (e.g., "GET", "POST").
		/// </summary>
		public const string RequestMethod = "owin.RequestMethod";
	
		/// <summary>
		/// A string containing the request path. The path MUST be relative to the "root" of the application delegate; see Paths.
		/// </summary>
		public const string RequestPath = "owin.RequestPath";
	
		/// <summary>
		/// A string containing the portion of the request path corresponding to the "root" of the application delegate; see Paths.
		/// </summary>
		public const string RequestPathBase = "owin.RequestPathBase";

		/// <summary>
		/// A string containing the protocol name and version (e.g. "HTTP/1.0" or "HTTP/1.1").
		/// </summary>
		public const string RequestProtocol = "owin.RequestProtocol";

		/// <summary>
		/// A string containing the query string component of the HTTP request URI, without the leading “?” (e.g., "foo=bar&baz=quux"). The value may be an empty string.
		/// </summary>
		public const string RequestQueryString = "owin.RequestQueryString";
	
		/// <summary>
		/// A string containing the URI scheme used for the request (e.g., "http", "https"); see URI Scheme. 
		/// </summary>
		public const string RequestScheme = "owin.RequestScheme";


		/// <summary>
		/// A Stream used to write out the response body, if any. See Response Body.
		/// </summary>
		public const string Response = "owin.ResponseBody";

		/// <summary>
		/// An IDictionary<string, string[]> of response headers.  See Headers.
		/// </summary>
		public const string ResponseHeaders = "owin.ResponseHeaders";
	
		/// <summary>
		/// An optional int containing the HTTP response status code as defined in RFC 2616 section 6.1.1. The default is 200.
		/// </summary>
		public const string ResponseStatusCode = "owin.ResponseStatusCode";
	
		/// <summary>
		/// An optional string containing the reason phrase associated the given status code. If none is provided then the server SHOULD provide a default as described in RFC 2616 section 6.1.1
		/// </summary>
		public const string ResponseReasonPhrase = "owin.ResponseReasonPhrase";
	
		/// <summary>
		/// An optional string containing the protocol name and version (e.g. "HTTP/1.0" or "HTTP/1.1"). If none is provided then the “owin.RequestProtocol” key’s value is the default.
		/// </summary>
		public const string ResponseProtocol = "owin.ResponseProtocol";
	

		/// <summary>
		/// A CancellationToken indicating if the request has been cancelled/aborted. See Request Lifetime.
		/// </summary>
		public const string CancellationToken = "owin.CallCancelled";
	
		/// <summary>
		/// The string "1.0" indicating OWIN version. See Versioning.
		/// </summary>
		public const string Version = "owin.Version";


		/// <summary>
		/// Global capabilities that do not change on a per-request basis.  See Section 5 above.
		/// IDictionary<string, object>
		/// </summary>
		public const string ServerCapabilities = "server.Capabilities";

		/// <summary>
		/// A list of per-address server configuration.  The following keys are defined with string values: scheme, host, port, path.
		/// IList<IDictionary<string, object>>
		/// </summary>
		public const string HostAddresses = "host.Addresses";

		/// <summary>
		/// A tracing output that may be provided by the host.
		/// TextWriter
		/// </summary>
		public const string Trace = "host.TraceOutput";
	

		/// <summary>
		/// String
		/// The IP Address of the remote client. E.g. 192.168.1.1 or ::1
		/// </summary>
		public const string RemoteIpAddress = "server.RemoteIpAddress";

		/// <summary>
		/// String
		/// The port of the remote client. E.g. 1234
		/// </summary>
		public const string RemotePort = "server.RemotePort";
	
		/// <summary>
		/// String
		/// The local IP Address the request was received on. E.g. 127.0.0.1 or ::1
		/// </summary>
		public const string LocalIpAddress = "server.LocalIpAddress";

		/// <summary>
		/// String
		/// The port the request was received on. E.g. 80
		/// </summary>
		public const string LocalPort = "server.LocalPort";

		/// <summary>
		/// Boolean
		/// Was the request sent from the same machine? E.g. true or false.
		/// </summary>
		public const string IsRequestLocal = "server.IsLocal";


		public const string User = "server.User";
	}
}

