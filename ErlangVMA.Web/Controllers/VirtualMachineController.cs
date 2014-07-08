using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace ErlangVMA
{
    public class VirtualMachineController : Controller
    {
        [HttpGet]
        public ActionResult List()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create()
        {
            return RedirectToAction("List");
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Shutdown(int id)
        {
            return RedirectToAction("List");
        }
    }
}
