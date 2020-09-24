using Orchard;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Mvc.Html;
using System;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.Services {
    public class ThemeHelperService : IThemeHelperService {
        private readonly ShellSettings _shellSettings;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IHomeAliasService _homeAliasService;
        private readonly ILocalizationService _localizationService;

        public ThemeHelperService(
            ShellSettings shellSettings,
            IWorkContextAccessor workContextAccessor,
            IHomeAliasService homeAliasService,
            ILocalizationService localizationService) {

            _shellSettings = shellSettings;
            _workContextAccessor = workContextAccessor;
            _homeAliasService = homeAliasService;
            _localizationService = localizationService;
        }

        public string UrlPrefix {
            get {
                var prefix = _shellSettings.RequestUrlPrefix;
                if (!String.IsNullOrWhiteSpace(prefix)) {
                    prefix += "/";
                }
                return prefix;
            }
        }

        UrlHelper Url {
            get {
                return new UrlHelper(
                    _workContextAccessor.GetContext()
                    .HttpContext.Request.RequestContext);
            }
        }

        public string GetLocalizedHomeUrl() {
            var currentCulture = _workContextAccessor.GetContext().CurrentCulture;
            var home = _homeAliasService.GetHomePage();
            if (home.Is<LocalizationPart>()
                && home.As<LocalizationPart>().Culture != null
                && home.As<LocalizationPart>().Culture.Culture != currentCulture) {

                home = _localizationService.GetLocalizedContentItem(home, currentCulture);
            }
            return home != null ? Url.ItemDisplayUrl(home) : ProperUrl("");
        }

        public string ProperUrl(string path) {

            return Url.Content("~/" + UrlPrefix + "/"
                + path.TrimStart('/').TrimStart('~').TrimStart('/'));
        }
    }
}