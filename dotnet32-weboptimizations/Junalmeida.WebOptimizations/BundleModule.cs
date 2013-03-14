using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Configuration;

namespace Junalmeida.WebOptimizations
{
    /// <summary>
    /// Bundle module is responsible to combine and minify bundles.
    /// Insert the module configuration before other modules.
    /// </summary>
    public class BundleModule : IHttpModule
    {

        public void Init(HttpApplication application)
        {
            if (application == null)
                throw new ArgumentNullException("application");
            application.PostResolveRequestCache += new EventHandler(this.OnApplicationPostResolveRequestCache);
        }

        private void OnApplicationPostResolveRequestCache(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            if (BundleTable.Bundles.Count > 0)
            {
                HttpContextBase context = (HttpContextBase)new HttpContextWrapper(app.Context);
                string virtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                VirtualPathProvider virtualPathProvider = HostingEnvironment.VirtualPathProvider;
                if (!virtualPathProvider.FileExists(virtualPath)) // && !virtualPathProvider.DirectoryExists(virtualPath))
                {
                    Bundle bundle = BundleTable.Bundles.GetBundleFor(virtualPath);
                    if (bundle != null)
                    {
                        bundle.Execute(app.Context, virtualPath);
                        //return true;
                    }
                }
                
                //return false;
            }
        }


        public void Dispose()
        {
        }

        private static BundleModule Current { get; set; }

        internal static void Check()
        {
            if (Current != null)
                return;
            if (HttpContext.Current == null || HttpContext.Current.ApplicationInstance == null || HttpContext.Current.ApplicationInstance.Modules == null)
                return;

            foreach (var key in HttpContext.Current.ApplicationInstance.Modules.AllKeys)
            {
                var httpModule = HttpContext.Current.ApplicationInstance.Modules[key];
                if (httpModule.GetType() == typeof(BundleModule))
                {
                    Current = (BundleModule)httpModule;
                    break;
                }
            }
            if (Current == null)
                throw new ConfigurationException("Missing BundleModule configuration on your web.config. Insert BundleModule configuration before other modules.");
        }
    }
}