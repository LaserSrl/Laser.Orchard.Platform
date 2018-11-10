using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Mvc.Extensions;

namespace Laser.Orchard.StartupConfig.Extensions {
    /// <summary>
    /// TODO: (PH:Autoroute) Many of these are or could be redundant (see controllers)
    /// </summary>
    public static class MediaHelperExtensions {
        public static string MediaExtensionsImageUrl(this UrlHelper urlHelper) {
            return urlHelper.Action("Image", "MediaTransform", new { area = "Laser.Orchard.StartupConfig" });
        }

    }
}