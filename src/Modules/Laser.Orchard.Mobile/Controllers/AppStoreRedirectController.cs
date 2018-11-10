using Laser.Orchard.Mobile.Services;
using System;
using System.Web.Mvc;

namespace Laser.Orchard.Mobile.Controllers {
    public class AppStoreRedirectController : Controller {
        private readonly IUserAgentRedirectServices _userAgentRedirectServices;
        public AppStoreRedirectController(IUserAgentRedirectServices userAgentRedirectServices) {
            _userAgentRedirectServices = userAgentRedirectServices;
        }
        public new RedirectResult Url(string url, string appName) {
            _userAgentRedirectServices.PersistAnswer(appName);
            if (String.IsNullOrWhiteSpace(url)) {
                url = Request.UrlReferrer.ToString();
            }
            return new RedirectResult(url);
        }

    }
}