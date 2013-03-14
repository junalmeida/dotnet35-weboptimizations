using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Junalmeida.WebOptimizations;

namespace Mvc2Test
{
    static class BundleConfig
    {
        internal static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/Scripts/bundletest").Include(
                "~/Scripts/jquery-1.4.1.js",
                "~/Scripts/test-script1.js",
                "~/Scripts/test-script2.js"
                ));
#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}