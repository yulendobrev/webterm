using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security;

namespace ErlangVMA
{
	public class LoginModel
	{
		[Required]
		public string Name
		{
			get;
			set;
		}

		[Required]
		public string Password
		{
			get;
			set;
		}

		[DefaultValue(false)]
		public bool Remember
		{
			get;
			set;
		}
	}
}

