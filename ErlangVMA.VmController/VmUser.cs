using System;

namespace ErlangVMA.VmController
{
	public class VmUser
	{
		private string username;

		public VmUser() : this("")
		{
		}

		public VmUser(string username)
		{
			this.username = username;
		}

		public string Username
		{
			get { return username; }
			set { username = value; }
		}

		public override bool Equals(object obj)
		{
			var user = obj as VmUser;

			return (object)user != null && username == user.username;
		}

		public override int GetHashCode()
		{
			return username.GetHashCode();
		}

		public static bool operator==(VmUser user, VmUser otherUser)
		{
			return ((object)user == null && (object)otherUser == null) || ((object)user != null && user.Equals(otherUser));
		}

		public static bool operator!=(VmUser user, VmUser otherUser)
		{
			return !(user == otherUser);
		}
	}
}

