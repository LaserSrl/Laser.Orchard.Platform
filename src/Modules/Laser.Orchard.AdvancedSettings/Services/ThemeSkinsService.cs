using Laser.Orchard.AdvancedSettings.Models;
using Laser.Orchard.AdvancedSettings.ViewModels;
using Newtonsoft.Json;
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

        public ThemeSkinsService(
            IVirtualPathProvider virtualPathProvider,
            ISiteThemeService siteThemeService,
            IAdvancedSettingsService advancedSettingsService,
            IResourceManager resourceManager) {
            
            _virtualPathProvider = virtualPathProvider;
            _siteThemeService = siteThemeService;
            _advancedSettingsService = advancedSettingsService;
            _resourceManager = resourceManager;
        }
        
        private SkinsManifest _skinsManifest;
        public SkinsManifest GetSkinsManifest() {
            if (_skinsManifest == null) {
                _skinsManifest = new SkinsManifest();
                var themePath = GetThemePath();
                // find the manifest
                var manifestFile = PathCombine(themePath, "skinsconfig.json");
                if (_virtualPathProvider.FileExists(manifestFile)) {
                    using (var manifestStream = _virtualPathProvider.OpenFile(manifestFile)) {
                        using (var reader = new StreamReader(manifestStream)) {
                            _skinsManifest = JsonConvert.DeserializeObject<SkinsManifest>(reader.ReadToEnd());
                        }
                    }
                }
            }
            return _skinsManifest;
        }
        
        public IEnumerable<string> GetSkinNames() {
            var manifest = GetSkinsManifest();
            return manifest.Skins.Select(s => s.Name);
        }

        public IEnumerable<ThemeCssVariable> GetSkinVariables() {
            var manifest = GetSkinsManifest();
            return manifest.Variables;
        }

        protected string GetThemePath() {
            // get current frontend theme
            var theme = _siteThemeService.GetSiteTheme();
            // find the Styles/Skins folder for the theme
            var basePath = PathCombine(theme.Location, theme.Id);
            return basePath;
        }
        protected string GetSkinStylesPath() {
            var basePath = GetThemePath();
            var stylesPath = PathCombine(basePath, "Styles");
            var skinsPath = PathCombine(stylesPath, "Skins");
            return skinsPath;
        }
        protected string GetSkinScriptsPath() {
            var basePath = GetThemePath();
            var scriptsPath = PathCombine(basePath, "Scripts");
            var skinsPath = PathCombine(scriptsPath, "Skins");
            return skinsPath;
        }

        protected ThemeSkinsPart GetConfigurationPart(string settingsName) {
            var settingsCI = _advancedSettingsService.GetCachedSetting(settingsName);
            if (settingsCI != null) {
                var skinPart = settingsCI.As<ThemeSkinsPart>();
                return skinPart;
            }
            return null;
        }

        // From the name of the settings CI, get the name of the skin/stylesheet
        protected string GetSelectedSkin(string settingsName) {
            var skinPart = GetConfigurationPart(settingsName);
            if (skinPart != null) {
                return skinPart.SkinName;
            }
            // no additional skin is configured
            return null;
        }

        protected string GetStyleSheet(string skinName, bool minified = false) {
            return GetResourceFile(skinName, ".css", GetSkinStylesPath(), minified);
        }
        protected string GetScript(string skinName, bool minified = false) {
            return GetResourceFile(skinName, ".js", GetSkinScriptsPath(), minified);
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
            if (manifest != null && skinPart != null) {
                var selectedSkin = manifest.Skins.FirstOrDefault(tsd => tsd.Name.Equals(skinPart.SkinName));
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