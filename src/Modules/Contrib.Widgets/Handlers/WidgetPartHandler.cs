using Contrib.Widgets.Models;
using Contrib.Widgets.Settings;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contrib.Widgets.Handlers {
    public class WidgetPartHandler : ContentHandler {
        private readonly IWorkContextAccessor _workContext;
        private readonly IContentManager _contentManager;
        public WidgetPartHandler(IWorkContextAccessor workContext, IContentManager contentManager) {
            _workContext = workContext;
            _contentManager = contentManager;
            OnGetEditorShape<WidgetPart>((context, part) => {
                
                var wexPart = part.As<WidgetExPart>();
                var hostId = (wexPart == null)
                    ? (int?)null
                    : wexPart.Record.HostId;

                if ((!hostId.HasValue || hostId.Value == 0) &&
                _workContext.GetContext().HttpContext.Items["CurrentController"] is Contrib.Widgets.Controllers.AdminController) {
                    if (!String.IsNullOrEmpty(_workContext.GetContext().HttpContext.Request.QueryString["hostId"])) {
                        hostId = Convert.ToInt32(_workContext.GetContext().HttpContext.Request.QueryString["hostId"]);
                    }
                }

                if (hostId.HasValue && hostId > 0) {
                    var widgetContainerPart = part.ContentItem.ContentManager.Get(hostId.Value, VersionOptions.Latest)
                            .As<WidgetsContainerPart>();
                    WidgetsContainerSettings settings = new WidgetsContainerSettings();
                    if (widgetContainerPart != null) {
                        settings = widgetContainerPart.Settings.GetModel<WidgetsContainerSettings>();
                    }

                    if (settings != null) {
                        var currentThemesZones = ((IEnumerable<string>)part.AvailableZones);
                        if (!settings.UseHierarchicalAssociation) {
                            if (!string.IsNullOrWhiteSpace(settings.AllowedZones)) {
                                currentThemesZones = currentThemesZones.Where(x => settings.AllowedZones.Split(',').Contains(x)).ToList();
                            }
                        } else if (settings.HierarchicalAssociation != null && settings.HierarchicalAssociation.Count() > 0) {
                            currentThemesZones = currentThemesZones.Where(ctz => settings.HierarchicalAssociation.Select(x => x.ZoneName)
                                .Contains(ctz)).ToList();
                        }

                        part.AvailableZones = currentThemesZones;
                    }
                }
            }
            );
        }
    }
}