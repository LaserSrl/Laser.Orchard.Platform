using Laser.Orchard.AdvancedSettings.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;
using Orchard.Themes.Services;
using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Laser.Orchard.AdvancedSettings.Services {
    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class ThemeSkinsService : IThemeSkinsService {
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly ISiteThemeService _siteThemeService;
        private readonly IAdvancedSettingsService _advancedSettingsService;

        public ThemeSkinsService(
            IVirtualPathProvider virtualPathProvider,
            ISiteThemeService siteThemeService,
            IAdvancedSettingsService advancedSettingsService) {
            
            _virtualPathProvider = virtualPathProvider;
            _siteThemeService = siteThemeService;
            _advancedSettingsService = advancedSettingsService;
        }
        
        // based on the ThemeSkinsPart, find the css to load.
        
        // Read the /Styles/Skins folders for the current theme. Find the
        // css files to be used as skins. In the future we'll be able to get
        // also files with the same name and different extensions to implement
        // additiona sub features, like displaying a preview or a description.
        public IEnumerable<string> GetSkinNames() {
            var skinsPath = GetSkinsPath();
            // find the css files
            var styleSheets = _virtualPathProvider.ListFiles(skinsPath)
                .Select(Path.GetFileName)
                .Where(fileName => string.Equals(
                    Path.GetExtension(fileName),
                    ".css",
                    StringComparison.OrdinalIgnoreCase))
                .Select(fileName => Path.GetFileNameWithoutExtension(fileName))
                // remove the .min for minified files
                .Where(n => !n.EndsWith(".min"))
                // make sure we are not providing both the "normal" and minified
                .Distinct()
                ;
            return styleSheets;
        }

        protected string GetSkinsPath() {
            // get current frontend theme
            var theme = _siteThemeService.GetSiteTheme();
            // find the Styles/Skins folder for the theme
            var basePath = Path.Combine(theme.Location, theme.Id).Replace(Path.DirectorySeparatorChar, '/');
            var stylesPath = Path.Combine(basePath, "Styles").Replace(Path.DirectorySeparatorChar, '/');
            var skinsPath = Path.Combine(stylesPath, "Skins").Replace(Path.DirectorySeparatorChar, '/');
            return skinsPath;
        }

        // From the name of the settings CI, get the name of the skin/stylesheet
        protected string GetSelectedSkin(string settingsName) {
            var settingsCI = _advancedSettingsService.GetCachedSetting(settingsName);
            if (settingsCI != null) {
                var skinPart = settingsCI.As<ThemeSkinsPart>();
                if (skinPart != null) {
                    return skinPart.SkinName;
                }
            }
            // no additional css is configured
            return null;
        }

        protected string GetStyleSheet(string skinName, bool minified = false) {
            var filename = skinName;
            if (minified) {
                filename += ".min.css";
            } else {
                filename += ".css";
            }
            var filePath = Path.Combine(GetSkinsPath(), filename).Replace(Path.DirectorySeparatorChar, '/');
            if(!_virtualPathProvider.FileExists(filePath)) {
                // File not found
                return null;
            }
            return filePath;
        }

        public void IncludeSkin(ResourceRegister Style, ResourceRegister Script, string settingsName) {
            var skinName = GetSelectedSkin(settingsName);
            if (!string.IsNullOrWhiteSpace(skinName)) {
                var debugPath = GetStyleSheet(skinName);
                var resourcePath = GetStyleSheet(skinName, true);
                if (string.IsNullOrWhiteSpace(resourcePath)) {
                    resourcePath = debugPath;
                }
                if (!string.IsNullOrWhiteSpace(resourcePath)) {
                    Style.Include(debugPath, resourcePath).AtHead();
                }
                // TODO: manage having also scripts
            }
        }
    }
}