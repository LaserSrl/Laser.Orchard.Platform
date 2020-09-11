using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.Extensions {
    public static class UrlHelperExtensions {
        private  const string OrchardUsersArea = "Orchard.Users";

        public static string LogOn(this UrlHelper urlHelper, string returnUrl) {
            if (!string.IsNullOrEmpty(returnUrl))
                return urlHelper.Action("LogOn", "Account", new { area = OrchardUsersArea, ReturnUrl = returnUrl });
            return urlHelper.Action("LogOn", "Account", new { area = OrchardUsersArea });
        }

        public static string LogOn(this UrlHelper urlHelper, string returnUrl, string userName, string loginData) {
            if (!string.IsNullOrEmpty(returnUrl))
                return urlHelper.Action("LogOn", "Account", new { area = OrchardUsersArea, ReturnUrl = returnUrl, UserName = userName, ExternalLoginData = loginData });
            return urlHelper.Action("LogOn", "Account", new { area = OrchardUsersArea, UserName = userName, ExternalLoginData = loginData });
        }

        public static string LogOff(this UrlHelper urlHelper, string returnUrl) {
            if (!string.IsNullOrEmpty(returnUrl))
                return urlHelper.Action("LogOff", "Account", new { area = OrchardUsersArea, ReturnUrl = returnUrl });
            return urlHelper.Action("LogOff", "Account", new { area = OrchardUsersArea });
        }

        public static string Register(this UrlHelper urlHelper, string userName, string loginData) {
            return urlHelper.Action("Register", "Account", new { area = OrchardUsersArea, UserName = userName, ExternalLoginData = loginData });
        }
    }
}