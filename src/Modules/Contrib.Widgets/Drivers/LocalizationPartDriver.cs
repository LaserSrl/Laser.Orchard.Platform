using System;
using Contrib.Widgets.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization.Models;
using Orchard.Logging;

namespace Contrib.Widgets.Drivers
{
    public class LocalizationPartDriver : ContentPartDriver<LocalizationPart> {
        private const string TemplatePrefix = "Contrib.Widget";
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContext;
        public ILogger Logger { get; set; }
        public LocalizationPartDriver(IContentManager contentManager, IWorkContextAccessor workContext) {
            _contentManager = contentManager;
            _workContext = workContext;
            Logger = NullLogger.Instance;
        }


        protected override DriverResult Editor(LocalizationPart part, dynamic shapeHelper) {
            var widgetExPart = part.As<WidgetExPart>() == null ? null : part.As<WidgetExPart>();
            var hostId = (int?)null;
            if (widgetExPart == null) {
                //the localization part is not used in widgets 
                return null;
            }

            if (widgetExPart.Record != null) {
                hostId = widgetExPart.Record.HostId;
            }

            if (!hostId.HasValue) {
                if (_workContext.GetContext().HttpContext.Items["CurrentController"] is Contrib.Widgets.Controllers.AdminController) {
                    if (!String.IsNullOrEmpty(_workContext.GetContext().HttpContext.Request.QueryString["hostId"])) {
                        hostId = Convert.ToInt32(_workContext.GetContext().HttpContext.Request.QueryString["hostId"]);
                    }
                }
            }
            Logger.Error($"Debug - ctype: {part.ContentItem.ContentType}, hostId.HasValue: {hostId.HasValue}");
            LocalizationPart hostLocPart = null;
            if (hostId.HasValue) {
                hostLocPart = _contentManager.Get<LocalizationPart>(hostId.Value, VersionOptions.Latest);
                Logger.Error($"Debug - hostId: {hostId.Value}");
            }
            var hostCulture = (hostLocPart != null) ? ((hostLocPart.Culture != null) ? hostLocPart.Culture.Culture : "") : "";
            Logger.Error($"Debug - hostCulture: {hostCulture}");
            return ContentShape("Parts_ContribWidget_Localization_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts/ContribWidget.Localization.Edit", Model: hostCulture, Prefix: TemplatePrefix));
        }
    }
}