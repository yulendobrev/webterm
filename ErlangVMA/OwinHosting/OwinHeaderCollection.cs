using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;

namespace ErlangVMA
{
	public class OwinHeaderCollection : IDictionary<string, string[]>
	{
		private NameValueCollection collection;

		public OwinHeaderCollection (NameValueCollection collection)
		{
			this.collection = collection;
		}

		public void Add (string key, string[] values)
		{
			foreach (var value in values)
				collection.Add (key, value);
		}

		public bool Remove(string key)
		{
			collection.Remove(key);
			return true;
		}

		public bool ContainsKey(string key)
		{
			return collection.GetValues(key) == null;
		}

		public bool TryGetValue (string key, out string[] value)
		{
			value = null;
			try
			{
				value = collection.GetValues (key);
				return true;
			}
			catch
			{ }

			return false;
		}

		public string[] this [string key]
		{
			get
			{
				return collection.GetValues(key);
			}
			set
			{
				Remove(key);
				Add(key, value);
			}
		}

		public ICollection<string> Keys
		{
			get
			{
				return collection.AllKeys;
			}
		}

		public ICollection<string[]> Values
		{
			get
			{
				return null;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IEnumerable<KeyValuePair<string, string[]>>)this).GetEnumerator();
		}

		IEnumerator<KeyValuePair<string, string[]>> IEnumerable<KeyValuePair<string, string[]>>.GetEnumerator ()
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

		public void Add (KeyValuePair<string, string[]> item)
		{
			Add (item.Key, item.Value);
		}

		public void Clear ()
		{
			collection.Clear();
		}

		public bool Contains (KeyValuePair<string, string[]> item)
		{
			string[] value;

			if (TryGetValue (item.Key, out value) && (value != null && value.Equals (item.Value)))
				return true;

			return false;
		}

		public void CopyTo (KeyValuePair<string, string[]>[] array, int arrayIndex)
		{
		}

		public bool Remove (KeyValuePair<string, string[]> item)
		{
			if (Contains (item))
				return Remove (item.Key);

			return false;
		}
	}
}

