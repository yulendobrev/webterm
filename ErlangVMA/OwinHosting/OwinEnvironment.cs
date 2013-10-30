using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Collections;

namespace ErlangVMA
{
	public class OwinEnvironment : IDictionary<string, object>
	{
		private Dictionary<string, object> extraDictionary;

		private HttpApplication app;

		private HttpRequest request;
		private HttpResponse response;
		private HttpContext context; 

		public OwinEnvironment (HttpApplication app)
		{
			this.extraDictionary = new Dictionary<string, object>();

			this.app = app;

			this["host.AppName"] = "Erlang VMA";

			this[OwinEnvironmentKeys.ServerCapabilities] = new Dictionary<string, object>();
			this[OwinEnvironmentKeys.HostAddresses] = new List<Dictionary<string, object>>();
			//this[OwinEnvironmentKeys.Trace] = TextWriter.Null;
		}

		public OwinEnvironment CreateRequestEnvironment (HttpContext context)
		{
			var env = new OwinEnvironment (app);

			env.request = context.Request;
			env.response = context.Response;
			env.context = context;

			return env;
		}

		public void Add (string key, object value)
		{
			if (IsBuiltinKey(key))
				throw new InvalidOperationException();

			extraDictionary.Add(key, value);
		}

		public bool Remove(string key)
		{
			if (IsBuiltinKey(key))
				return false;

			return extraDictionary.Remove(key);
		}

		public bool ContainsKey(string key)
		{
			return IsBuiltinKey(key) || extraDictionary.ContainsKey(key);
		}

		public bool TryGetValue (string key, out object value)
		{
			value = null;

			switch (key) {
			case OwinEnvironmentKeys.Request:
				if (request != null)
					value = request.InputStream;
				break;
			case OwinEnvironmentKeys.RequestHeaders:
				if (request != null)
					value = new OwinHeaderCollection(request.Headers);
				break;
			case OwinEnvironmentKeys.RequestMethod:
				if (request != null)
					value = request.HttpMethod;
				break;
			case OwinEnvironmentKeys.RequestPath:
				if (request != null)
					value = request.CurrentExecutionFilePath;
				break;
			case OwinEnvironmentKeys.RequestPathBase:
				if (request != null)
					value = request.ApplicationPath.TrimEnd('/');
				break;
			case OwinEnvironmentKeys.RequestProtocol:
				if (request != null)
					value = "HTTP/1.1";
				break;
			case OwinEnvironmentKeys.RequestQueryString:
				if (request != null)
				{
					string query = request.Url.Query;
					if (!string.IsNullOrEmpty(query))
						query = query.Substring(1);

					value = query;
				}
				break;
			case OwinEnvironmentKeys.RequestScheme:
				if (request != null)
					value = request.Url.Scheme;
				break;

			case OwinEnvironmentKeys.Response:
				if (response != null)
					value = response.OutputStream;
				break;
			case OwinEnvironmentKeys.ResponseHeaders:
				if (response != null)
					value = new OwinHeaderCollection(response.Headers);
				break;
			case OwinEnvironmentKeys.ResponseProtocol:
				if (response != null)
					value = "HTTP/1.1";
				break;
			case OwinEnvironmentKeys.ResponseReasonPhrase:
				if (response != null)
					value = response.StatusDescription;
				break;
			case OwinEnvironmentKeys.ResponseStatusCode:
				if (response != null)
					value = response.StatusCode;
				break;

			case OwinEnvironmentKeys.Version:
				value = "1.0";
				break;
			//case OwinEnvironmentKeys.ServerCapabilities:
			case OwinEnvironmentKeys.Trace:
				value = TextWriter.Null;
				break;

			//case OwinEnvironmentKeys.RemotePort:
			//case OwinEnvironmentKeys.RemoteIpAddress:

			case OwinEnvironmentKeys.IsRequestLocal:
				if (request != null)
					value = request.IsLocal;
				break;
			case OwinEnvironmentKeys.User:
				if (context != null)
					value = context.User;
				break;
			case OwinEnvironmentKeys.LocalIpAddress:
			case OwinEnvironmentKeys.LocalPort:
			default:
				return extraDictionary.TryGetValue (key, out value);
			}

			return true;
		}

		public object this [string key]
		{
			get
			{
				object value;
				if (!TryGetValue(key, out value))
					throw new KeyNotFoundException();

				return value;
			}
			set
			{
				if (!IsBuiltinKey(key))
				{
					extraDictionary[key] = value;
				}
				else
				{
					switch (key)
					{
					case OwinEnvironmentKeys.ResponseReasonPhrase:
						if (response != null)
							response.StatusDescription = value.ToString();
						break;
					case OwinEnvironmentKeys.ResponseStatusCode:
						if (response != null)
							response.StatusCode = (int)value;
						break;
					default:
						throw new InvalidOperationException(string.Format("Cannot set key {0}", key));
					}
				}
			}
		}

		private bool IsBuiltinKey (string key)
		{
			switch (key)
			{
			case OwinEnvironmentKeys.Request:
			case OwinEnvironmentKeys.RequestHeaders:
			case OwinEnvironmentKeys.RequestMethod:
			case OwinEnvironmentKeys.RequestPath:
			case OwinEnvironmentKeys.RequestPathBase:
			case OwinEnvironmentKeys.RequestProtocol:
			case OwinEnvironmentKeys.RequestQueryString:
			case OwinEnvironmentKeys.RequestScheme:

			case OwinEnvironmentKeys.Response:
			case OwinEnvironmentKeys.ResponseHeaders:
			case OwinEnvironmentKeys.ResponseProtocol:
			case OwinEnvironmentKeys.ResponseReasonPhrase:
			case OwinEnvironmentKeys.ResponseStatusCode:

			case OwinEnvironmentKeys.Version:
			//case OwinEnvironmentKeys.ServerCapabilities:
			case OwinEnvironmentKeys.Trace:

			case OwinEnvironmentKeys.RemotePort:
			case OwinEnvironmentKeys.RemoteIpAddress:

			case OwinEnvironmentKeys.IsRequestLocal:
			case OwinEnvironmentKeys.LocalIpAddress:
			case OwinEnvironmentKeys.LocalPort:
				return true;
			}

			return false;
		}

		private bool IsReadOnlyKey (string key)
		{
			switch (key)
			{
			case OwinEnvironmentKeys.Request:
			case OwinEnvironmentKeys.RequestHeaders:
			case OwinEnvironmentKeys.RequestMethod:
			case OwinEnvironmentKeys.RequestPath:
			case OwinEnvironmentKeys.RequestPathBase:
			case OwinEnvironmentKeys.RequestProtocol:
			case OwinEnvironmentKeys.RequestQueryString:
			case OwinEnvironmentKeys.RequestScheme:

			case OwinEnvironmentKeys.Response:
			case OwinEnvironmentKeys.ResponseHeaders:
			case OwinEnvironmentKeys.ResponseProtocol:

			case OwinEnvironmentKeys.Version:
			case OwinEnvironmentKeys.ServerCapabilities:
			case OwinEnvironmentKeys.Trace:

			case OwinEnvironmentKeys.RemotePort:
			case OwinEnvironmentKeys.RemoteIpAddress:

			case OwinEnvironmentKeys.IsRequestLocal:
			case OwinEnvironmentKeys.LocalIpAddress:
			case OwinEnvironmentKeys.LocalPort:
				return true;
			}

			return false;
		}

		public ICollection<string> Keys
		{
			get
			{
				return extraDictionary.Keys;
			}
		}

		public ICollection<object> Values
		{
			get
			{
				return extraDictionary.Values;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IEnumerable<KeyValuePair<string, object>>)this).GetEnumerator();
		}

		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator ()
		{
			return null;
		}

		public int Count
		{
			get { return 0; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public void Add (KeyValuePair<string, object> item)
		{
			Add (item.Key, item.Value);
		}

		public void Clear ()
		{
			extraDictionary.Clear();
		}

		public bool Contains (KeyValuePair<string, object> item)
		{
			object value;

			if (TryGetValue (item.Key, out value) && (value != null && value.Equals (item.Value)))
				return true;

			return false;
		}

		public void CopyTo (KeyValuePair<string, object>[] array, int arrayIndex)
		{
		}

		public bool Remove (KeyValuePair<string, object> item)
		{
			if (Contains (item))
				return Remove (item.Key);

			return false;
		}
	}
}

