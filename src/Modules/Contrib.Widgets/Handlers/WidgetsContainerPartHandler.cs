using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Contrib.Widgets.Models;
using Contrib.Widgets.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization.Models;

namespace Contrib.Widgets.Handlers {
    public class WidgetsContainerPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly IWidgetManager _widgetManager;

        public WidgetsContainerPartHandler(IContentManager contentManager, IWidgetManager widgetManager) {
            _contentManager = contentManager;
            _widgetManager = widgetManager;

            OnRemoved<WidgetsContainerPart>((context, part) => {
                DeleteWidgets(part);
            });

            OnUpdated<WidgetsContainerPart>((context, part) => {
                var culture = part.ContentItem.As<LocalizationPart>().Culture;
                // recupero i WidgetExPart del Content
                var widgets = _widgetManager.GetWidgets(part.ContentItem.Id, part.ContentItem.IsPublished());
                foreach (var widget in widgets) {
                    var localization = widget.ContentItem.As<LocalizationPart>();
                    localization.Culture = culture;
                }
            });
        }


        private void DeleteWidgets(WidgetsContainerPart part) {
            var contentItem = part.ContentItem;

            var widgets = _widgetManager.GetWidgets(contentItem.Id, false);
            foreach (var w in widgets) {
                _contentManager.Remove(w.ContentItem);
            }
        }
    }
}