using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Junalmeida.WebOptimizations
{
    public static class Styles
    {
        public static MvcHtmlString Render(params string[] paths)
        {
            return AssetManager.BuildTags(paths, System.Web.UI.HtmlTextWriterTag.Link);
        }
    }
}