using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Descriptor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.TinyMceEnhancement {
    public interface ITinyMceEnhancementService : IDependency {
        string GetDefaultInitScript();
        string GetFrontendDefaultInitScript();
        string GetCorePluginsList();
        string GetFrontendCorePluginsList();
    }
    public class TinyMceEnhancementService : ITinyMceEnhancementService {
        private enum TinyMceContextOptions { FrontEnd, Admin };
        private readonly IOrchardServices _orchardServices;
        private ShellDescriptor _shellDescriptor;
        public TinyMceEnhancementService(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            _shellDescriptor = _orchardServices.WorkContext.Resolve<ShellDescriptor>();

        }
        public string GetCorePluginsList() {
            var toolbarPlugins = GetMediaToolbarPlugins();
            return Constants.BasePlugins + toolbarPlugins.Item2;
        }
        public string GetFrontendCorePluginsList() {
            var toolbarPlugins = GetFrontendMediaToolbarPlugins();
            return Constants.FrontendBasePlugins + toolbarPlugins.Item2;
        }
        public string GetDefaultInitScript() {
            var toolbarPlugins = GetMediaToolbarPlugins();
            var plugins = Constants.BasePlugins + toolbarPlugins.Item2;
            return Constants.BasePartialInit + @",
                    toolbar: """ + ReorganizeToolbar(Constants.BaseLeftToolbar + toolbarPlugins.Item1 + Constants.BaseRightToolbar, TinyMceContextOptions.Admin) + @""",
                    plugins: [""" + ReorganizePlugins(plugins, TinyMceContextOptions.Admin) + @"""]";
        }
        public string GetFrontendDefaultInitScript() {
            var toolbarPlugins = GetFrontendMediaToolbarPlugins();
            var plugins = Constants.FrontendBasePlugins + toolbarPlugins.Item2;
            return Constants.FrontendBasePartialInit + @",
                    toolbar: """ + ReorganizeToolbar(Constants.FrontendBaseLeftToolbar + toolbarPlugins.Item1 + Constants.FrontendBaseRightToolbar, TinyMceContextOptions.FrontEnd) + @""",
                    plugins: [""" + ReorganizePlugins(plugins, TinyMceContextOptions.FrontEnd) + @"""]";
        }


        /// <summary>
        /// Adds the plugins that depend from orchard features
        /// </summary>
        /// <param name="currentPlugins">a string representing the current plugin list</param>
        /// <param name="context">Admin or FrontEnd</param>
        /// <returns>a string representing the new reorganized plugin list</returns>
        private string ReorganizePlugins(string currentPlugins, TinyMceContextOptions context) {
            var settings = _orchardServices.WorkContext.CurrentSite.As<TinyMceSiteSettingsPart>();
            if (context == TinyMceContextOptions.Admin) {
                var contenPickerEnabled = (_shellDescriptor.Features.Any(x => x.Name == "Orchard.ContentPicker") ? true : false);
                var tokensHtmlFilterEnabled = (_shellDescriptor.Features.Any(x => x.Name == "Orchard.Tokens.HtmlFilter") ? true : false);
                if (contenPickerEnabled && tokensHtmlFilterEnabled && !currentPlugins.Contains("orchardcontentlinks")) {
                    currentPlugins += ", orchardcontentlinks";
                }
            }
            return currentPlugins;
        }

        /// <summary>
        /// Adds the toolbar buttons that depend from orchard features
        /// </summary>
        /// <param name="currentToolbar">a string representing the current toolbar buttons list</param>
        /// <param name="context">Admin or FrontEnd</param>
        /// <returns>a string representing the new reorganized toolbar buttons list</returns>
        private string ReorganizeToolbar(string currentToolbar, TinyMceContextOptions context) {
            if (context == TinyMceContextOptions.Admin) {

                var contenPickerEnabled = (_shellDescriptor.Features.Any(x => x.Name == "Orchard.ContentPicker") ? true : false);
                var tokensHtmlFilterEnabled = (_shellDescriptor.Features.Any(x => x.Name == "Orchard.Tokens.HtmlFilter") ? true : false);
                if (contenPickerEnabled && tokensHtmlFilterEnabled && !currentToolbar.Contains("orchardlink")) {
                    currentToolbar = currentToolbar.Replace(" link ", " link orchardlink ");
                }
            }
            return currentToolbar;

        }

        private Tuple<string, string> GetMediaToolbarPlugins() {
            var mediaPickerEnabled = _shellDescriptor.Features.Any(x => x.Name == "Orchard.MediaPicker") ? true : false;
            var mediaLibraryEnabled = _shellDescriptor.Features.Any(x => x.Name == "Orchard.MediaLibrary") ? true : false;
            var mediaPlugins = "";
            var mediaToolbar = "";
            if (mediaPickerEnabled) {
                mediaPlugins += ", mediapicker";
                mediaToolbar += " mediapicker";
            }
            if (mediaLibraryEnabled) {
                mediaPlugins += ", medialibrary";
                mediaToolbar += " medialibrary";
            }
            return new Tuple<string, string>(mediaToolbar, mediaPlugins);
        }

        private Tuple<string, string> GetFrontendMediaToolbarPlugins() {
            var mediaPickerEnabled = false;
            var mediaLibraryEnabled = false;
            var mediaPlugins = "";
            var mediaToolbar = "";
            if (mediaPickerEnabled) {
                mediaPlugins += ", mediapicker";
                mediaToolbar += " mediapicker";
            }
            if (mediaLibraryEnabled) {
                mediaPlugins += ", medialibrary";
                mediaToolbar += " medialibrary";
            }
            return new Tuple<string, string>(mediaToolbar, mediaPlugins);
        }
    }
}