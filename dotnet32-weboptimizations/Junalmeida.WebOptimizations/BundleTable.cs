using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Junalmeida.WebOptimizations
{
    public static class BundleTable
    {
        static BundleTable()
        {
            Bundles = new BundleCollection();
            MapPathMethod = HttpContext.Current.Server.MapPath;
        }

        private static BundleModule module;
        private static BundleCollection bundles;
        public static BundleCollection Bundles
        {
            get
            {
                BundleModule.Check();
                return bundles;
            }
            private set
            {
                bundles = value;
            }
        }
        public static bool EnableOptimizations { get; set; }
        public static Func<string, string> MapPathMethod { get; set; }
    }
}