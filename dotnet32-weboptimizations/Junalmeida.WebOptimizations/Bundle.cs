using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.IO.Compression;

namespace Junalmeida.WebOptimizations
{
    public abstract class Bundle
    {
        protected Bundle()
        {
            items = new List<string>();
        }

        public Bundle(string virtualPath)
            : this()
        {
            key = virtualPath;
        }

        private List<string> items;
        private string key;

        public string VirtualPath
        {
            get { return key; }
            set { key = value; }
        }

        public virtual Bundle Include(params string[] virtualPaths)
        {
            foreach (var file in virtualPaths)
                File.GetAttributes(BundleTable.MapPathMethod(file)); //exception if some file does not exist.
           
            items.AddRange(virtualPaths);
            return this;
        }

        //public Bundle IncludeDirectory(string directoryVirtualPath, string searchPattern)
        //{
        //    return IncludeDirectory(directoryVirtualPath, searchPattern, false);
        //}

        //public Bundle IncludeDirectory(string directoryVirtualPath, string searchPattern, bool searchSubdirectories)
        //{
        //    return this;
        //}
        public virtual string[] EnumerateFiles()
        {
            return items.ToArray();
        }

        protected abstract string ContentType { get; }



        public override bool Equals(object obj)
        {
            var other = obj as Bundle;
            if (obj == null)
                return false;
            else
                return string.Equals(VirtualPath, other.VirtualPath, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return VirtualPath.ToLower().GetHashCode();
        }




        protected abstract string Minify(string file);

        internal void Execute(System.Web.HttpContext context, string virtualPath)
        {
            string cacheKey = "bundle_" + virtualPath;
            string cacheDatesKey = "bundle_dates_" + virtualPath;
            string cacheGenerated = "bundle_generated_" + virtualPath;

            StringBuilder builder = null;
            DateTime[] filesModified = null;
            var currentFilesDates = GetFilesLastModified();
            var currentModifiedDate = (DateTime?)null;
            var lastUserDate = (DateTime?)null;

            {
                string headerIfModified = context.Request.Headers["If-Modified-Since"];
                DateTime lastUserDateHeader;
                if (DateTime.TryParse(headerIfModified, out lastUserDateHeader))
                    lastUserDate = lastUserDateHeader;
            }

            if (context.Cache != null)
            {
                builder = context.Cache.Get(cacheKey) as StringBuilder;
                filesModified = context.Cache.Get(cacheDatesKey) as DateTime[];
                currentModifiedDate = context.Cache.Get(cacheGenerated) as DateTime?;
            }
            //var currentDate = currentFilesDates.First();

            if (builder == null || filesModified == null || currentModifiedDate == null || 
                !filesModified.SequenceEqual(currentFilesDates))
            {
                //gerar bundle miny
                builder = new StringBuilder();
                BundleAndMinify(builder);
                currentModifiedDate = DateTime.Today.AddSeconds(Math.Floor((DateTime.Now - DateTime.Today).TotalSeconds));
                if (context.Cache != null)
                {
                    context.Cache.Remove(cacheKey);
                    context.Cache.Remove(cacheDatesKey);
                    context.Cache.Remove(cacheGenerated);
                    context.Cache.Add(cacheKey, builder, null,
                        System.Web.Caching.Cache.NoAbsoluteExpiration,
                        System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
                    context.Cache.Add(cacheDatesKey, currentFilesDates, null,
                        System.Web.Caching.Cache.NoAbsoluteExpiration,
                        System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
                    context.Cache.Add(cacheGenerated, currentModifiedDate, null,
                        System.Web.Caching.Cache.NoAbsoluteExpiration,
                        System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
                }
            }


            context.Response.Clear();
            if (DateTime.Equals(currentModifiedDate, lastUserDate))
            {
                context.Response.StatusCode = 304;
                context.Response.SuppressContent = true;
            }
            else
            {
                context.Response.ContentType = this.ContentType;
                var cacheResponse = context.Response.Cache;
                cacheResponse.SetLastModified(currentModifiedDate.Value);

                context.Response.Write(builder.ToString());

                SetGzip(context);
            }
            context.Response.End();
        }

        private void SetGzip(HttpContext context)
        {
            context.Response.Cache.VaryByHeaders["Accept-Encoding"] = true;


            string acceptEncoding = context.Request.Headers["Accept-Encoding"];

            if (string.IsNullOrEmpty(acceptEncoding)) return;

            acceptEncoding = acceptEncoding.ToUpperInvariant();

            if (acceptEncoding.Contains("GZIP"))
            {
                //isCompressed = true;
                context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
                context.Response.AddHeader("Content-Encoding", "gzip");
            }
            else if (acceptEncoding.Contains("DEFLATE"))
            {
                //isCompressed = true;
                context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress);
                context.Response.AddHeader("Content-Encoding", "deflate");
            }
        }

        private DateTime[] GetFilesLastModified()
        {
            return
                (from f in items
                 select new FileInfo(BundleTable.MapPathMethod(f)).LastWriteTime
                     ).ToArray();
        }

        private void BundleAndMinify(StringBuilder builder)
        {
            foreach (var file in items)
            {
                builder.AppendLine(this.Minify(file));
            }
        }
    }

    public class ScriptBundle : Bundle
    {
        public ScriptBundle(string virtualPath) 
            : base (virtualPath)
        {
            settings = new Microsoft.Ajax.Utilities.CodeSettings()
            {
                PreserveImportantComments = false
                //everything else default;
            };

        }
        Microsoft.Ajax.Utilities.CodeSettings settings;

        protected override string ContentType
        {
            get
            {
                return "text/javascript";
            }
        }


        protected override string Minify(string file)
        {
            //using AjaxMin as minifyer. less dependencies.
            var physicalFile = BundleTable.MapPathMethod(file);
            var minifier = new Microsoft.Ajax.Utilities.Minifier();
            var content = File.ReadAllText(physicalFile);

            string result;
            if (file.ToLower().EndsWith(".min.js"))
                result = minifier.MinifyJavaScript(content, new Microsoft.Ajax.Utilities.CodeSettings() { 
                    MinifyCode=false,
                    PreserveImportantComments = false
                });
            else
                result = minifier.MinifyJavaScript(content, settings);
            return result + ";";
        }
    }

    public class StyleBundle : Bundle
    {
        public StyleBundle(string virtualPath)
            : base(virtualPath)
        {
            settings = new Microsoft.Ajax.Utilities.CssSettings()
            {
                CommentMode = Microsoft.Ajax.Utilities.CssComment.None
                //everything else default;
            };

        }


        protected override string ContentType
        {
            get
            {
                return "text/css";
            }
        }

        Microsoft.Ajax.Utilities.CssSettings settings;

        protected override string Minify(string file)
        {
            //using AjaxMin as minifyer. less dependencies.
            var physicalFile = BundleTable.MapPathMethod(file);
            var minifier = new Microsoft.Ajax.Utilities.Minifier();
            var content = File.ReadAllText(physicalFile);
            if (file.ToLower().EndsWith(".min.css"))
                return content;

            var result = minifier.MinifyStyleSheet(content, settings);
            return result;
        }
    }
}
