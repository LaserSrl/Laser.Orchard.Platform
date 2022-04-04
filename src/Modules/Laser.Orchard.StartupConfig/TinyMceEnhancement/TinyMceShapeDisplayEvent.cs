using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;
using Orchard.UI.Admin;

namespace Laser.Orchard.StartupConfig.TinyMceEnhancement {
    [OrchardFeature("Laser.Orchard.StartupConfig.TinyMceEnhancement")]
    public class TinyMceShapeDisplayEvent : ShapeDisplayEvents {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private readonly ITinyMceEnhancementService _tinyMceEnhancementService;
        public TinyMceShapeDisplayEvent(IOrchardServices orchardServices, ShellSettings shellSettings, ITinyMceEnhancementService tinyMceEnhancementService) {
            _orchardServices = orchardServices;
            _shellSettings = shellSettings;
            _tinyMceEnhancementService = tinyMceEnhancementService;
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
            
            bool isAdmin = AdminFilter.IsApplied(HttpContext.Current.Request.RequestContext);

            var settings = _orchardServices.WorkContext.CurrentSite.As<TinyMceSiteSettingsPart>();

            if (isAdmin) {
                var htmlOrig = context.ChildContent.ToHtmlString();
                var plugins = _tinyMceEnhancementService.GetCorePluginsList();
                var externalPlugins = "";
                var scriptList = new List<string>();
                // There are two different AdditionalPlugins fields: 
                // - AdditionalPlugins are used for backoffice plugins
                // - FrontendAdditionalPlugins are used for frontend content edit (in this case, plugins needing admin panel access authorization need to be disabled)
                var additionalPlugins = settings.AdditionalPlugins;

                if (!string.IsNullOrWhiteSpace(additionalPlugins)) {
                    // external plugins example:
                    // external_plugins: {
                    //   'testing': 'http://www.testing.com/plugin.min.js',
                    //   'maths': 'http://www.maths.com/plugin.min.js'
                    // }
                    var namesList = additionalPlugins.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    var pluginList = new List<string>();
                    var serverRelativeUrlBase = string.IsNullOrWhiteSpace(HttpContext.Current.Request.ApplicationPath) ? "" : "/" + HttpContext.Current.Request.ApplicationPath.TrimStart('/');
                    foreach (var item in namesList) {
                        if (item.Contains('/')) {
                            //scriptList.Add($"<script src=\"{serverRelativeUrlBase}/Modules/Laser.Orchard.StartupConfig/Scripts/tinymceplugins/{item}\" type=\"text/javascript\"></script>");
                            var pluginName = item.Split('/')[0];
                            pluginList.Add($"'{pluginName}': '{serverRelativeUrlBase}/Modules/Laser.Orchard.StartupConfig/Scripts/tinymceplugins/{item}'");
                        } else {
                            pluginList.Add($"'{item}': '{serverRelativeUrlBase}/Modules/Laser.Orchard.StartupConfig/Scripts/tinymceplugins/{item}/plugin.min.js'");
                        }
                    }
                    externalPlugins = @",
                    external_plugins: {
                        " + string.Join(",\r\n", pluginList) + @"
                    }";
                }
                var init = "";
                // There are two different InitScript fields: 
                // - InitScript is used for backoffice plugins
                // - FrontendInitScript is used for frontend content edit (in this case, plugins needing admin panel access authorization need to be disabled)
                var initScript = settings.InitScript;
                if (string.IsNullOrWhiteSpace(initScript)) {
                    init = _tinyMceEnhancementService.GetDefaultInitScript();
                } else {
                    init = initScript + @",
                    plugins: [""" + plugins + @"""]";
                }
                var additionalScripts = new StringBuilder();
                foreach (var script in scriptList) {
                    additionalScripts.AppendLine(script);
                }
                var html = @"<script type=""text/javascript"">
                $(function() {
                    tinyMCE.init({
                        " + init + externalPlugins + @",
                        " + Constants.DefaultSetup + @"
                    });
                });
                </script>
                " + additionalScripts.ToString() + htmlOrig;
                context.ChildContent = new HtmlString(html);
            } else {
                FrontendDisplayed(context, settings);
            }
        }

        private void FrontendDisplayed(ShapeDisplayedContext context, TinyMceSiteSettingsPart settings) {
            var htmlOrig = context.ChildContent.ToHtmlString();
            var plugins = _tinyMceEnhancementService.GetFrontendCorePluginsList();
            var externalPlugins = "";
            var scriptList = new List<string>();
            // There are two different AdditionalPlugins fields: 
            // - AdditionalPlugins are used for backoffice plugins
            // - FrontendAdditionalPlugins are used for frontend content edit (in this case, plugins needing admin panel access authorization need to be disabled)
            var additionalPlugins = settings.FrontendAdditionalPlugins;

            if (!string.IsNullOrWhiteSpace(additionalPlugins)) {
                // external plugins example:
                // external_plugins: {
                //   'testing': 'http://www.testing.com/plugin.min.js',
                //   'maths': 'http://www.maths.com/plugin.min.js'
                // }
                var namesList = additionalPlugins.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                var pluginList = new List<string>();
                var serverRelativeUrlBase = string.IsNullOrWhiteSpace(HttpContext.Current.Request.ApplicationPath) ? "" : "/" + HttpContext.Current.Request.ApplicationPath.TrimStart('/');
                foreach (var item in namesList) {
                    if (item.Contains('/')) {
                        //scriptList.Add($"<script src=\"{serverRelativeUrlBase}/Modules/Laser.Orchard.StartupConfig/Scripts/tinymceplugins/{item}\" type=\"text/javascript\"></script>");
                        var pluginName = item.Split('/')[0];
                        pluginList.Add($"'{pluginName}': '{serverRelativeUrlBase}/Modules/Laser.Orchard.StartupConfig/Scripts/tinymceplugins/{item}'");
                    } else {
                        pluginList.Add($"'{item}': '{serverRelativeUrlBase}/Modules/Laser.Orchard.StartupConfig/Scripts/tinymceplugins/{item}/plugin.min.js'");
                    }
                }
                externalPlugins = @",
                    external_plugins: {
                        " + string.Join(",\r\n", pluginList) + @"
                    }";
            }
            var init = "";
            // There are two different InitScript fields: 
            // - InitScript is used for backoffice plugins
            // - FrontendInitScript is used for frontend content edit (in this case, plugins needing admin panel access authorization need to be disabled)
            var initScript = settings.FrontendInitScript;
            if (string.IsNullOrWhiteSpace(initScript)) {
                init = _tinyMceEnhancementService.GetFrontendDefaultInitScript();
            } else {
                init = initScript + @",
                    plugins: [""" + plugins + @"""]";
            }
            var additionalScripts = new StringBuilder();
            foreach (var script in scriptList) {
                additionalScripts.AppendLine(script);
            }
            var html = @"<script type=""text/javascript"">
                $(function() {
                    tinyMCE.init({
                        " + init + externalPlugins + @",
                        " + Constants.FrontendDefaultSetup + @"
                    });
                });
                </script>
                " + additionalScripts.ToString() + htmlOrig;
            context.ChildContent = new HtmlString(html);
        }
    }
}
 