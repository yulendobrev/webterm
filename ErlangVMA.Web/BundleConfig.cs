using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace ErlangVMA.Web
{
    public static class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundleCollection)
        {
            bundleCollection.Add(new ScriptBundle("~/jquery")
                .Include("~/Scripts/jquery-{version}.js")
                .Include("~/Scripts/jquery.signalR-{version}.js")
                .Include("~/signalr/hubs"));

            bundleCollection.Add(new ScriptBundle("~/vmconsole")
                .Include("~/Scripts/vmconsole.js"));

            bundleCollection.Add(new StyleBundle("~/default")
                .Include("~/Styles/Default.css"));

            BundleTable.EnableOptimizations = false;
        }
    }
}