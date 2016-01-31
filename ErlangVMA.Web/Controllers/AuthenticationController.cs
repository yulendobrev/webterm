using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Security;

namespace ErlangVMA.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View(new LoginModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //if (Membership.ValidateUser(model.Name, model.Password))
                {
                    Request.GetOwinContext().Authentication.SignIn(new Microsoft.Owin.Security.AuthenticationProperties
                    {
                        IsPersistent = model.Remember
                    }, new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, model.Name) }, "ApplicationCookie"));

                    return Redirect(!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View(new RegisterModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //FormsAuthentication.SetAuthCookie(model.UserName, false);
            return Redirect("/");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Request.GetOwinContext().Authentication.SignOut("ApplicationCookie");

            return Redirect("/");
        }
    }
}
