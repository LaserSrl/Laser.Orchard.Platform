using Orchard;
using Orchard.Environment.Descriptor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.TinyMceEnhancement {
    public interface ITinyMceEnhancementService : IDependency {
        string GetDefaultInitScript();
        string GetCorePluginsList();
    }
    public class TinyMceEnhancementService : ITinyMceEnhancementService {
        private readonly IOrchardServices _orchardServices;
        public TinyMceEnhancementService(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }
        public string GetCorePluginsList() {
            var toolbarPlugins = GetMediaToolbarPlugins();
            return Constants.BasePlugins + toolbarPlugins.Item2;
        }
        public string GetDefaultInitScript() {
            var toolbarPlugins = GetMediaToolbarPlugins();
            var plugins = Constants.BasePlugins + toolbarPlugins.Item2;
            return Constants.BasePartialInit + @",
                    toolbar: """ + Constants.BaseLeftToolbar + toolbarPlugins.Item1 + Constants.BaseRightToolbar + @""",
                    plugins: [""" + plugins + @"""]";
        }
        private Tuple<string, string> GetMediaToolbarPlugins() {
            var shellDescriptor = _orchardServices.WorkContext.Resolve<ShellDescriptor>();
            var mediaPickerEnabled = shellDescriptor.Features.Any(x => x.Name == "Orchard.MediaPicker") ? true : false;
            var mediaLibraryEnabled = shellDescriptor.Features.Any(x => x.Name == "Orchard.MediaLibrary") ? true : false;
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