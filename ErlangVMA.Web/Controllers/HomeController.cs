using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace ErlangVMA.Web.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			ViewData["Message"] = "Welcome to ASP.NET MVC on Mono!";
			return View();
		}

		public ActionResult VirtualMachine()
		{
			return View();
		}
	}
}
