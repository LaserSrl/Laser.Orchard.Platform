using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Laser.Orchard.OpenAuthentication.Extensions {
    public static class UrlHelperExtensions {

        public static string OpenAuthLogOn(this UrlHelper urlHelper, string returnUrl) {
            return urlHelper.Action("ExternalLogOn", "Account", new { area = Constants.LocalArea, ReturnUrl = returnUrl });
        }

        public static string Referer(this UrlHelper urlHelper, HttpRequestBase httpRequestBase) {
            if (httpRequestBase.UrlReferrer != null) {
                return httpRequestBase.UrlReferrer.ToString();
            }
            return "~/";
        }

        public static string RemoveProviderConfiguration(this UrlHelper urlHelper, int id) {
            return urlHelper.Action("Remove", "Admin", new { area = Constants.LocalArea, Id = id });
        }

        public static string EditProviderConfiguration(this UrlHelper urlHelper, int id) {
            return urlHelper.Action("Edit", "Admin", new { area = Constants.LocalArea, Id = id });
        }

        public static string ProviderCreate(this UrlHelper urlHelper) {
            return urlHelper.Action("CreateProvider", "Admin", new { area = Constants.LocalArea });
        }
    }
}