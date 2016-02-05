using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using ErlangVMA.VmController;

namespace ErlangVMA.Web.Controllers
{
    [Authorize]
    public class VirtualMachineController : Controller
    {
        private readonly IVmBroker vmBroker;

        public VirtualMachineController(IVmBroker vmBroker)
        {
            this.vmBroker = vmBroker;
        }

        [HttpGet]
        public ActionResult List()
        {
            var virtualMachines = vmBroker.GetVirtualMachines(GetCurrentUser());

            return View(virtualMachines);
        }

        [HttpGet]
        public ActionResult StartNew()
        {
            return View(new VirtualMachineStartOptions());
        }

        [HttpPost]
        public ActionResult StartNew(VirtualMachineStartOptions startOptions)
        {
            var stopwatch = Stopwatch.StartNew();

            if (!ModelState.IsValid)
            {
                return View(startOptions);
            }

            int virtualMachineId = vmBroker.StartNewNode(GetCurrentUser(), startOptions);

            Debug.WriteLine("Starting new virtual machine took {0} ms", stopwatch.ElapsedMilliseconds);

            return RedirectToAction("Interact", new { id = virtualMachineId });
        }

        [HttpGet]
        public ActionResult Interact(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("List");
            }
            return View(id.Value);
        }

        [HttpGet]
        public ActionResult Details(int? id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Shutdown(int id, string returnUrl)
        {
            vmBroker.ShutdownNode(GetCurrentUser(), id);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("List");
        }

        private VmUser GetCurrentUser()
        {
            return new VmUser(HttpContext.User.Identity.Name);
        }
    }
}
