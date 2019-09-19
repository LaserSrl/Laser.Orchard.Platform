using Orchard;
using Orchard.Autoroute.Models;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CulturePicker.Services {
    public class ContentLocalizableRouteService : ILocalizableRouteService {
        private readonly ILocalizableContentService _localizableContentService;

        public ContentLocalizableRouteService(ILocalizableContentService localizableContentService,
                                              IOrchardServices services) {
            _localizableContentService = localizableContentService;
            Services = services;

        }

        public int Priority {
            get {
                return 0;
            }
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }


        public bool TryFindLocalizedUrl(LocalizableRouteContext localizableRouteContext) {
            bool translatedUrlFound = false;

            if (string.IsNullOrEmpty(localizableRouteContext.Culture)) {
                return translatedUrlFound;
            }
            var urlPrefix = Services.WorkContext.Resolve<ShellSettings>().RequestUrlPrefix;

            string returnUrl = Utils.GetCleanUrl(
                Services.WorkContext.HttpContext.Request, 
                localizableRouteContext.UrlToLocalize,
                urlPrefix);

            AutoroutePart currentRoutePart;
            //returnUrl may not correspond to any content and we use "Try" approach
            if (_localizableContentService.TryGetRouteForUrl(returnUrl, out currentRoutePart)) {
                AutoroutePart localizedRoutePart;
                //content may not have localized version and we use "Try" approach
                if (_localizableContentService.TryFindLocalizedRoute(currentRoutePart.ContentItem, localizableRouteContext.Culture, out localizedRoutePart)) {
                    returnUrl = localizedRoutePart.PromoteToHomePage ? "" : localizedRoutePart.Path;
                    if (!String.IsNullOrWhiteSpace(urlPrefix) && !returnUrl.StartsWith(urlPrefix)) {
                        returnUrl = "~/" + returnUrl;
                    }
                    localizableRouteContext.UrlLocalized = returnUrl;
                    translatedUrlFound = true;
                }
            }

            return translatedUrlFound;
        }
    }
}