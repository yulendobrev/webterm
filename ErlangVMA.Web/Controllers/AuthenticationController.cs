using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;

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
                var userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(new IdentityDbContext("VmNodesDbContext")));
                var user = userManager.FindAsync(model.Name, model.Password).Result;

                if (user != null)
                {
                    var authentication = Request.GetOwinContext().Authentication;
                    var identity = userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie).Result;

                    authentication.SignIn(new AuthenticationProperties { IsPersistent = model.Remember }, identity);

                    return Redirect(!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No user with the provided user name and password exists");
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
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Password confirmation failed");
            }

            if (ModelState.IsValid)
            {
                var userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(new IdentityDbContext("VmNodesDbContext")));
                var registration = userManager.CreateAsync(new IdentityUser { UserName = model.UserName }, model.Password).Result;

                if (registration.Succeeded)
                {
                    var authentication = Request.GetOwinContext().Authentication;

                    var user = userManager.FindAsync(model.UserName, model.Password).Result;
                    if (user != null)
                    {
                        var identity = userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie).Result;
                        authentication.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);

                        return Redirect("/");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "No user with the provided user name and password exists");
                    }
                }
                else
                {
                    foreach (string error in registration.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Request.GetOwinContext().Authentication.SignOut("ApplicationCookie");

            return Redirect("/");
        }
    }
}
