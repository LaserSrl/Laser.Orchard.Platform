using Orchard;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.MediaLibrary.Models;
using Orchard.MediaProcessing.Shapes;
using Orchard.Mvc.Html;
using System;
using System.IO;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.Services {
    public class ThemeHelperService : IThemeHelperService {
        private readonly ShellSettings _shellSettings;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IHomeAliasService _homeAliasService;
        private readonly ILocalizationService _localizationService;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IExtensionManager _extensionManager;
        private readonly MediaShapes _mediaShapes;

        public ThemeHelperService(
            ShellSettings shellSettings,
            IWorkContextAccessor workContextAccessor,
            IHomeAliasService homeAliasService,
            ILocalizationService localizationService,
            IVirtualPathProvider virtualPathProvider,
            IExtensionManager extensionManager,
            MediaShapes mediaShapes) {

            _shellSettings = shellSettings;
            _workContextAccessor = workContextAccessor;
            _homeAliasService = homeAliasService;
            _localizationService = localizationService;
            _virtualPathProvider = virtualPathProvider;
            _extensionManager = extensionManager;
            _mediaShapes = mediaShapes;
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
            // if path is already an absolute URL (e.g. the url of a page on another website)
            // we shouldn't try to fix it
            Uri uriResult;
            bool isAllowedAbsoluteUri = Uri.TryCreate(path, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp
                    || uriResult.Scheme == Uri.UriSchemeHttps
                    || uriResult.Scheme == Uri.UriSchemeMailto
                    || uriResult.Scheme == Uri.UriSchemeFtp);
            if (isAllowedAbsoluteUri) {
                return uriResult.ToString();
            }

            return Url.Content("~/" + UrlPrefix + "/"
                + path.TrimStart('/').TrimStart('~').TrimStart('/'));
        }

        public string ThemeAssetPath(HtmlHelper html, string relPath) {

            var workContext = _workContextAccessor.GetContext();
            var theme = workContext.CurrentTheme;
            while (theme != null) {
                var currentPath = html.ThemePath(theme, relPath);
                if (_virtualPathProvider.FileExists(currentPath)) {
                    return currentPath;
                }
                // "climb" to the base theme
                var baseThemeName = theme.BaseTheme;
                if (!string.IsNullOrWhiteSpace(baseThemeName)) {
                    theme = _extensionManager.GetExtension(baseThemeName);
                } else {
                    // if the theme had no base theme, end iterations
                    break;
                }
            }
            // using this file should cause a 404, that we can then debug
            return html.ThemePath(workContext.CurrentTheme, relPath);

        }
    }
}