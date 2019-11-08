using System;
using System.Collections.Generic;
using System.Linq;
using Contrib.Widgets.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Localization.ViewModels;

namespace Contrib.Widgets.Drivers {
    public class LocalizationPartDriver : ContentPartDriver<LocalizationPart> {
        private const string TemplatePrefix = "Contrib.Widget";
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContext;

        public LocalizationPartDriver(IContentManager contentManager, IWorkContextAccessor workContext) {
            _contentManager = contentManager;
            _workContext = workContext;
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
                if (!String.IsNullOrEmpty(_workContext.GetContext().HttpContext.Request.QueryString["hostId"])) {
                    hostId = Convert.ToInt32(_workContext.GetContext().HttpContext.Request.QueryString["hostId"]);
                }
            }

            LocalizationPart hostLocPart = null;
            if (hostId.HasValue) {
                hostLocPart = _contentManager.Get<LocalizationPart>(hostId.Value, VersionOptions.Latest);
            }
            var hostCulture = (hostLocPart != null) ? ((hostLocPart.Culture != null) ? hostLocPart.Culture.Culture : "") : "";

            return ContentShape("Parts_ContribWidget_Localization_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts/ContribWidget.Localization.Edit", Model: hostCulture, Prefix: TemplatePrefix));
        }
    }
}