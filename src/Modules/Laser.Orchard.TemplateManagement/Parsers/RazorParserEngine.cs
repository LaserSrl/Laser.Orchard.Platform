using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.Services;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Laser.Orchard.StartupConfig.RazorCodeExecution.Services;
using Orchard.Environment.Configuration;

namespace Laser.Orchard.TemplateManagement.Parsers {

    [OrchardFeature("Laser.Orchard.TemplateManagement.Parsers.Razor")]
    public class RazorParserEngine : ParserEngineBase {
        private readonly IRazorTemplateManager _razorTemplateManager;
        private readonly ShellSettings _shellSettings;

        public RazorParserEngine(
           IRazorTemplateManager razorTemplateManager,
            ShellSettings shellSettings) {
            //       _razorMachine = razorMachine;
            _shellSettings = shellSettings;
            _razorTemplateManager = razorTemplateManager;
        }

        public override string DisplayText {
            get { return "Razor Engine"; }
        }

        public override string LayoutBeacon {
            get { return "@RenderBody()"; }
        }

        public override string ParseTemplate(TemplatePart template, ParseTemplateContext context) {
            int versionrecordid = template.Record.ContentItemRecord.Versions.Where(x => x.Published).FirstOrDefault().Id;
            string key = _shellSettings.Name + versionrecordid.ToString();
            // var tr = new RazorTemplateManager();
            var layout = template.Layout;
            var templateContent = template.Text;
            //var viewBag = context.ViewBag;
            string layoutString = null;
            if (layout != null) {
                key+="_"+ template.Layout.Record.ContentItemRecord.Versions.Where(x => x.Published).FirstOrDefault().Id;
                layoutString = layout.Text;
              //  _razorTemplateManager.AddLayout("Layout" + key, layout.Text);
                templateContent = "@{ Layout = \"Layout" + key + "\"; }\r\n" + templateContent;
            }

            try {
                var tmpl = _razorTemplateManager.RunString(key, templateContent, context.Model, (Dictionary<string, object>)context.ViewBag, layoutString);
                return tmpl;
            }
            catch (Exception ex) {
                Logger.Log(LogLevel.Error, ex, "Failed to parse the {0} Razor template with layout {1}", template.Title, layout != null ? layout.Title : "[none]");
                return BuildErrorContent(ex, template, layout);
            }
        }

        private static string BuildErrorContent(Exception ex, TemplatePart templatePart, TemplatePart layout) {
            var sb = new StringBuilder();
            var currentException = ex;
            sb.AppendLine("Error On Template");
            while (currentException != null) {
                sb.AppendLine(currentException.Message);
                currentException = currentException.InnerException;
            }

            sb.AppendFormat("\r\nTemplate ({0}):\r\n", templatePart.Title);
            sb.AppendLine(templatePart.Text);

            if (layout != null) {
                sb.AppendFormat("\r\nLayout ({0}):\r\n", layout.Title);
                sb.AppendLine(layout.Text);
            }
            return sb.ToString();
        }
    }
}