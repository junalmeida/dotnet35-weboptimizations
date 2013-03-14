using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Junalmeida.WebOptimizations
{
    public class BundleCollection : HashSet<Bundle>
    {
        internal Bundle GetBundleFor(string bundleUrlFromContext)
        {
            var virtualPathProvider = HostingEnvironment.VirtualPathProvider;
            var foundBundle = 
                (from bundle in this
                 where string.Equals(bundle.VirtualPath, bundleUrlFromContext, StringComparison.OrdinalIgnoreCase)
                 select bundle).SingleOrDefault();

            if (foundBundle == null)
            {
                bundleUrlFromContext = bundleUrlFromContext.ToLower();
                var notMinUrl = bundleUrlFromContext.Replace(".min.", ".");
                if (bundleUrlFromContext.EndsWith(".min.js") && 
                    virtualPathProvider.FileExists(notMinUrl))
                {
                    foundBundle = new ScriptBundle(bundleUrlFromContext).Include(notMinUrl);
                }
                else if (bundleUrlFromContext.EndsWith(".min.css") &&
                    virtualPathProvider.FileExists(notMinUrl))
                {
                    foundBundle = new StyleBundle(bundleUrlFromContext).Include(notMinUrl);
                }
            }
            return foundBundle;
        }
    }
}