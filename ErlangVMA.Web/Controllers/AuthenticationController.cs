using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Security;

namespace ErlangVMA
{
	public class AuthenticationController : Controller
	{
		[HttpGet]
		public ActionResult Login()
		{
			return View(new LoginModel());
		}

		[HttpPost]
		public ActionResult Login(LoginModel model)
		{
			if (ModelState.IsValid)
			{
				//if (Membership.ValidateUser (model.Name, model.Password))
				{
					FormsAuthentication.SetAuthCookie(model.Name, model.Remember);
					return Redirect(Request.QueryString["ReturnUrl"] ?? "/");
				}
			}

			return View(model);
		}

		[HttpGet]
		public ActionResult Logout()
		{
			FormsAuthentication.SignOut();

			return Redirect("/");
		}
	}
}
