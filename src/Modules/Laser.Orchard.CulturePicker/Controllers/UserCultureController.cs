using Laser.Orchard.CulturePicker.Services;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.CulturePicker.Controllers {
    public class UserCultureController : Controller {
        private readonly ILocalizableContentService _localizableContentService;
        private readonly ICulturePickerServices _cpServices;
        private IEnumerable<ILocalizableRouteService> _localizableRouteService;

        public UserCultureController(
            IOrchardServices services,
            IEnumerable<ILocalizableRouteService> localizableRouteService,
            ICulturePickerServices cpServices) {

            Services = services;
            _localizableRouteService = localizableRouteService;
            _cpServices = cpServices;
        }

        public IOrchardServices Services { get; set; }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult ChangeCulture(string cultureName) {
            var translatedUrlFound = false;
            if (Services.WorkContext.HttpContext.Request.UrlReferrer == null) {
                return new HttpStatusCodeResult(404);
            }
            if (string.IsNullOrEmpty(cultureName)) {
                return new HttpStatusCodeResult(404);
            }
            var urlPrefix = Services.WorkContext.Resolve<ShellSettings>().RequestUrlPrefix;
            var requestUrl = Utils.GetReturnUrl(Services.WorkContext.HttpContext.Request, urlPrefix);
            var requestQuerystring = Services.WorkContext.HttpContext.Request.UrlReferrer.Query;
            var context = new LocalizableRouteContext(requestUrl, requestQuerystring, cultureName);
            foreach (var provider in _localizableRouteService.OrderBy(x => x.Priority)) {
                if (provider.TryFindLocalizedUrl(context)) {
                    translatedUrlFound = true;
                }
            }

            // Set the cookie even if a translatedUrl has not been found (for coeherence with the user choice)
            _cpServices.SaveCultureCookie(cultureName, Services.WorkContext.HttpContext);
            return this.RedirectLocal(context.RedirectLocalUrl);
        }

    }
}