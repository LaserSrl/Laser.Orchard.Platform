using Orchard;
using System;
using System.Web;

namespace Laser.Orchard.CulturePicker.Services {
    public class CulturePickerServices : ICulturePickerServices {
        private readonly IOrchardServices _orchardServices;
        private readonly IWorkContextAccessor _workContextAccessor;

        public CulturePickerServices(IOrchardServices orchardServices, IWorkContextAccessor workContextAccessor) {
            _orchardServices = orchardServices;
            _workContextAccessor = workContextAccessor;
        }
        public void SaveCultureCookie(string cultureName, HttpContextBase context) {
            HttpRequestBase request = context.Request;

            var cultureCookie = new HttpCookie(CookieCultureSelector.CultureCookieName);
            cultureCookie.Values.Add(CookieCultureSelector.CurrentCultureFieldName, cultureName);
            cultureCookie.Expires = DateTime.UtcNow.AddMonths(1);

            //setting up domain for cookie allows to share it to sub-domains as well
            //if non-default port is used, we consider it as a testing environment without sub-domains
            if (UseSubdomainCookie(context)) {
                // '.' prefix means, that cookie will be shared to sub-domains
                cultureCookie.Domain = "." + request.Url.Host;
            }
            context.Request.Cookies.Remove(cultureCookie.Name);
            context.Response.Cookies.Remove(cultureCookie.Name);
            context.Response.Cookies.Add(cultureCookie);
        }

        private bool UseSubdomainCookie(HttpContextBase context) {
            //TODO: write an actual implementation based, e.g., on a site setting
            return false;

            //Original code to test whether subdomain cookie should be used:
            ////setting up domain for cookie allows to share it to sub-domains as well
            ////if non-default port is used, we consider it as a testing environment without sub-domains
            //if (request.Url != null &&
            //    request.Url.IsDefaultPort &&
            //    !(request.Url.Host.ToUpperInvariant() == "LOCALHOST")) {
            //    // '.' prefix means, that cookie will be shared to sub-domains
            //    cultureCookie.Domain = "." + request.Url.Host;
            //}
        }

    }
}