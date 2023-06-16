using Laser.Orchard.AdvancedSettings.Models;
using Laser.Orchard.AdvancedSettings.ViewModels;
using Newtonsoft.Json;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;
using Orchard.Themes.Services;
using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Laser.Orchard.AdvancedSettings.Services {
    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class ThemeSkinsService : IThemeSkinsService {
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly ISiteThemeService _siteThemeService;
        private readonly IAdvancedSettingsService _advancedSettingsService;
        private readonly IResourceManager _resourceManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IExtensionManager _extensionManager;

        public ThemeSkinsService(
            IVirtualPathProvider virtualPathProvider,
            ISiteThemeService siteThemeService,
            IAdvancedSettingsService advancedSettingsService,
            IResourceManager resourceManager,
            IWorkContextAccessor workContextAccessor,
            IExtensionManager extensionManager) {
            
            _virtualPathProvider = virtualPathProvider;
            _siteThemeService = siteThemeService;
            _advancedSettingsService = advancedSettingsService;
            _resourceManager = resourceManager;
            _workContextAccessor = workContextAccessor;
            _extensionManager = extensionManager;
        }
        
        private SkinsManifest _skinsManifest;
        protected SkinsManifest GetSkinsManifest() {
            if (_skinsManifest == null) {
                _skinsManifest = new SkinsManifest();
                // Get the paths for the current theme and its base theme
                var themePaths = GetThemePaths();
                var manifestFiles = themePaths
                    .Select(p => PathCombine(p, "skinsconfig.json"))
                    .Where(_virtualPathProvider.FileExists);
                // get corresponding manifests if they exists
                var allManifests = manifestFiles
                    .Select(f => {
                        using (var manifestStream = _virtualPathProvider.OpenFile(f)) {
                            using (var reader = new StreamReader(manifestStream)) {
                                return JsonConvert.DeserializeObject<SkinsManifest>(reader.ReadToEnd());
                            }
                        }
                    });
                // merge manifests into a single one
                _skinsManifest = SkinsManifest.MergeManifests(allManifests.Where(m => m != null));
                _skinsManifest.ThemePaths = themePaths.ToList();
            }
            return _skinsManifest;
        }
        
        protected ThemeSkinsSettingsPart GetSettings() {
            return _workContextAccessor.GetContext()
                .CurrentSite.As<ThemeSkinsSettingsPart>();
        }

        public IEnumerable<string> GetSkinNames() {
            Func<string, bool> predicate = s => true;
            var settings = GetSettings();
            if (settings != null) {
                if (settings.AvailableSkinNames != null
                    && settings.AvailableSkinNames.Length > 0
                    && !settings.AvailableSkinNames.Contains(ThemeSkinsSettingsPart.AllSkinsValue)) {
                    predicate = s => settings.AvailableSkinNames.Contains(s);
                }
            }
            return GetAllSkinNames().Where(predicate);
        }

        public IEnumerable<string> GetAllSkinNames() {
            var manifest = GetSkinsManifest();
            return manifest.Skins.Select(s => s.Name);
        }

        public IEnumerable<ThemeCssVariable> GetSkinVariables() {
            var manifest = GetSkinsManifest();
            return manifest.Variables;
        }
                
        protected IEnumerable<string> GetThemePaths() {
            // get current frontend theme
            var theme = _siteThemeService.GetSiteTheme();
            while (theme != null) {
                // find the Styles/Skins folder for the theme
                var basePath = PathCombine(theme.Location, theme.Id);
                yield return basePath;
                // "climb" to the base theme
                var baseThemeName = theme.BaseTheme;
                if (!string.IsNullOrWhiteSpace(baseThemeName)) {
                    theme = _extensionManager.GetExtension(baseThemeName);
                } else {
                    // if the theme had no base theme, end iterations
                    break;
                }
            }
        }

        protected ThemeSkinsPart GetConfigurationPart(string settingsName) {
            var settingsCI = _advancedSettingsService.GetCachedSetting(settingsName);
            if (settingsCI != null) {
                var skinPart = settingsCI.As<ThemeSkinsPart>();
                return skinPart;
            }
            return null;
        }

        protected string GetStyleSheet(string skinName, bool minified = false) {
            // In the manifest we stored the list of theme paths we used to discover
            // the skins. We go through those again to find the resource now.
            var paths = GetSkinsManifest().ThemePaths
                .Select(p => PathCombine(p, "Styles")).Select(p => PathCombine(p, "Skins"));
            // We look for the resource in the /Styles/Skins subpath. However Orchard seems
            // to find alternates for it in the /Styles subpath.
            foreach (var path in paths) {
                var resourcePath = GetResourceFile(skinName, ".css", path, minified);
                // if we found the file, return this path, otherwise go to the next path
                if (_virtualPathProvider.FileExists(resourcePath)) {
                    return resourcePath;
                }
            }
            // fallbacks:
            // We did not find the file. We return the path as if it was supposed to be in
            // the current theme: this will end up being a 404 in the browser.
            return GetResourceFile(skinName, ".css", paths.First(), minified);
        }
        protected string GetScript(string skinName, bool minified = false) {
            // In the manifest we stored the list of theme paths we used to discover
            // the skins. We go through those again to find the resource now.
            var paths = GetSkinsManifest().ThemePaths
                .Select(p => PathCombine(p, "Scripts")).Select(p => PathCombine(p, "Skins"));
            foreach (var path in paths) {
                var resourcePath = GetResourceFile(skinName, ".js", path, minified);
                // if we found the file, return this path, otherwise go to the next path
                if (_virtualPathProvider.FileExists(resourcePath)) {
                    return resourcePath;
                }
            }
            // fallbacks:
            // We did not find the file. We return the path as if it was supposed to be in
            // the current theme: this will end up being a 404 in the browser.
            return GetResourceFile(skinName, ".js", paths.First(), minified);
        }
        protected string GetResourceFile(string skinName, string extension, string path, bool minified = false) {
            var filename = skinName;
            if (minified) {
                filename += ".min" + extension;
            } else {
                filename += extension;
            }
            var filePath = PathCombine(path, filename);
            return filePath;
        }

        public void IncludeSkin(ResourceRegister Style, ResourceRegister Script, string settingsName) {
            var skinPart = GetConfigurationPart(settingsName);
            var manifest = GetSkinsManifest();
            var allowedSkinNames = GetSkinNames();
            if (manifest != null && skinPart != null) {
                var selectedSkin = manifest.Skins
                    .FirstOrDefault(tsd => allowedSkinNames.Contains(tsd.Name) 
                        && tsd.Name.Equals(skinPart.SkinName));
                // there may be a Default skin configured in the manifest, to be used
                // when there is nothing selected in the skinPart
                if (selectedSkin == null && string.IsNullOrWhiteSpace(skinPart.SkinName)) {
                    selectedSkin = manifest.Skins.FirstOrDefault(tsd => tsd.Name.Equals("Default", StringComparison.OrdinalIgnoreCase));
                }
                if (selectedSkin != null) {
                    // add css files to head of page
                    if (selectedSkin.StyleSheets != null) {
                        foreach (var cssName in selectedSkin.StyleSheets) {
                            var debugPath = GetStyleSheet(cssName);
                            var resourcePath = GetStyleSheet(cssName, true);
                            if (string.IsNullOrWhiteSpace(resourcePath)) {
                                resourcePath = debugPath;
                            }
                            if (!string.IsNullOrWhiteSpace(resourcePath)) {
                                Style.Include(debugPath, resourcePath).AtHead();
                            }
                        }
                    }
                    // add scripts to head of page
                    if (selectedSkin.HeadScripts != null) {
                        foreach (var scriptName in selectedSkin.HeadScripts) {
                            var debugPath = GetScript(scriptName);
                            var resourcePath = GetScript(scriptName, true);
                            if (string.IsNullOrWhiteSpace(resourcePath)) {
                                resourcePath = debugPath;
                            }
                            if (!string.IsNullOrWhiteSpace(resourcePath)) {
                                Script.Include(debugPath, resourcePath).AtHead();
                            }
                        }
                    }
                    // add scripts to foot of page
                    if (selectedSkin.FootScripts != null) {
                        foreach (var scriptName in selectedSkin.FootScripts) {
                            var debugPath = GetScript(scriptName);
                            var resourcePath = GetScript(scriptName, true);
                            if (string.IsNullOrWhiteSpace(resourcePath)) {
                                resourcePath = debugPath;
                            }
                            if (!string.IsNullOrWhiteSpace(resourcePath)) {
                                Script.Include(debugPath, resourcePath).AtFoot();
                            }
                        }
                    }
                }
                // add variables that are configured in the part
                var configuredVariables = skinPart.Variables.Where(v => !string.IsNullOrWhiteSpace(v.Value));
                if (configuredVariables.Any()) {
                    // create the style to add to the head of the page
                    var sb = new StringBuilder();
                    sb.AppendLine("<style>");
                    sb.AppendLine(":root {");
                    foreach (var variable in configuredVariables) {
                        sb.AppendLine(string.Format("{0}: {1};", variable.Name, variable.Value));
                    }
                    sb.AppendLine("}");
                    sb.AppendLine("</style>");
                    _resourceManager.RegisterHeadScript(sb.ToString());
                }
            }
        }

        // shortcut to methods
        private static string PathCombine(string path1, string path2) {
            return Path.Combine(path1, path2).Replace(Path.DirectorySeparatorChar, '/');
        }
    }
}