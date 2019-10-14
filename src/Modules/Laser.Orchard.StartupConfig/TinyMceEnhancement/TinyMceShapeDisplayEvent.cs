using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;

namespace Laser.Orchard.StartupConfig.TinyMceEnhancement {
    [OrchardFeature("Laser.Orchard.StartupConfig.TinyMceEnhancement")]
    public class TinyMceShapeDisplayEvent : ShapeDisplayEvents {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        public TinyMceShapeDisplayEvent(IOrchardServices orchardServices, ShellSettings shellSettings) {
            _orchardServices = orchardServices;
            _shellSettings = shellSettings;
        }
        public override void Displayed(ShapeDisplayedContext context) {
            // TODO aggiungere anche i field che usano TinyMce (verificare se serve)
            if (String.CompareOrdinal(context.ShapeMetadata.Type, "Body_Editor") != 0) {
                return;
            }

            if (!String.Equals(context.Shape.EditorFlavor, "html", StringComparison.InvariantCultureIgnoreCase)) {
                return;
            }
            base.Displayed(context);
            var settings = _orchardServices.WorkContext.CurrentSite.As<TinyMceSiteSettingsPart>();

            var htmlOrig = context.ChildContent.ToHtmlString();
            //html = @"<script>
            //        var lasertoolbar = 'undo redo cut copy paste | bold italic | bullist numlist outdent indent formatselect | alignleft aligncenter alignright alignjustify ltr rtl | ' + mediaPlugins + ' link unlink charmap | code fullscreen';
            //    </script>" + html;
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
            var plugins = Constants.BasePlugins + mediaPlugins;
            var externalPlugins = "";
            if (string.IsNullOrWhiteSpace(settings.AdditionalPlugins) == false) {
                // TODO: creare oggetto json con nome e url di ogni plugin aggiuntivo
                // predisporre cartella con web.config per mettere i plugin aggiuntivi e ricavare l'url
                // external plugins example:
                // external_plugins: {
                //   'testing': 'http://www.testing.com/plugin.min.js',
                //   'maths': 'http://www.maths.com/plugin.min.js'
                // }
                var namesList = settings.AdditionalPlugins.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                var pluginList = new List<string>();
                var shellSettings = new ShellSettings();
                var urlBase = _shellSettings.RequestUrlPrefix;
                if(string.IsNullOrWhiteSpace(urlBase) == false) {
                    urlBase = "/" + urlBase;
                }
                foreach (var item in namesList) {
                    pluginList.Add($"'{item}': '{urlBase}/{item}/plugin.min.js'");
                }
                externalPlugins = @",
                    external_plugins: {
                        " + string.Join(",\r\n", pluginList) + @"
                    }"; 
            }
            var init = "";
            if(string.IsNullOrWhiteSpace(settings.InitScript)) {
                init = Constants.BasePartialInit + @",
                    toolbar: """ + Constants.BaseLeftToolbar + mediaToolbar + Constants.BaseRightToolbar + @""",
                    plugins: [""" + plugins + @"""]";
            }
            else {
                init = settings.InitScript + @",
                    plugins: [""" + plugins + @"""]";
            }
            var html = @"<script type=""text/javascript"">
                $(function() {
                    tinyMCE.init({
                        " + init + externalPlugins + @",
                        " + Constants.DefaultSetup + @"
                    });
                });
                </script>" + htmlOrig;
            context.ChildContent = new HtmlString(html);
        }
    }
}
 