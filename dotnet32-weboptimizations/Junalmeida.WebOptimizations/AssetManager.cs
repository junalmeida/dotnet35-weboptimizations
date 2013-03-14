using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using System.Web.UI;
using System.Text;

namespace Junalmeida.WebOptimizations
{
    static class AssetManager
    {
        static AssetManager()
        {

        }

        public static string[] ResolveUrls(string virtualPath)
        {
            var physical = BundleTable.MapPathMethod(virtualPath);

            if (!BundleTable.EnableOptimizations && !File.Exists(physical))
            {
                //check if is bundle
                var bundle = BundleTable.Bundles.GetBundleFor(virtualPath);
                if (bundle != null)
                    return bundle.EnumerateFiles();
            }
            else if (File.Exists(physical) && BundleTable.EnableOptimizations)
            {
                var extension =Path.GetExtension(virtualPath).ToLower();
                if (!virtualPath.ToLower().EndsWith(".min" + extension))
                    return new string[] { virtualPath.Replace(extension, ".min" + extension) };
            }

            return new string[] { virtualPath };
        }

        internal static MvcHtmlString BuildTags(string[] virtualPaths, HtmlTextWriterTag tag)
        {
            var builder = new StringBuilder();

            foreach (var virtualPath in virtualPaths)
            {
                var urls = ResolveUrls(virtualPath);
                foreach (var url in urls)
                    builder.AppendLine(BuildOneTag(url, tag));
            }

            return MvcHtmlString.Create(builder.ToString());
        }

        internal static string BuildOneTag(string virtualPath, HtmlTextWriterTag tag)
        { 

            TagRenderMode tagRenderMode;

            var tagBuilder = new TagBuilder(tag.ToString().ToLower());

            var absolutePath = VirtualPathUtility.ToAbsolute(virtualPath);
            switch (tag)
            {
                case HtmlTextWriterTag.Script:
                    tagRenderMode = TagRenderMode.Normal;
                    tagBuilder.MergeAttribute("type", "text/javascript");
                    tagBuilder.MergeAttribute("src", absolutePath);
                    break;
                case HtmlTextWriterTag.Link:
                    tagRenderMode = TagRenderMode.SelfClosing;
                    tagBuilder.MergeAttribute("type", "text/css");
                    tagBuilder.MergeAttribute("media", "all"); //always ALL?
                    tagBuilder.MergeAttribute("rel", "stylesheet");
                    tagBuilder.MergeAttribute("href", absolutePath);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            
            return tagBuilder.ToString(tagRenderMode);
        }
    }
}