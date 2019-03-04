using Contrib.Widgets.Models;
using Contrib.Widgets.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization.Models;
using System.Linq;

namespace Contrib.Widgets.Handlers {
    public class WidgetsContainerPartLocalizationHandler : ContentHandler {

        private readonly IWidgetManager _widgetManager;
        private readonly IWorkContextAccessor _wca;

        public WidgetsContainerPartLocalizationHandler (
            IWidgetManager widgetManager,
            IWorkContextAccessor wca) {

            _widgetManager = widgetManager;
            _wca = wca;

            OnCloned<WidgetsContainerPart>(ResetWidgetsLocalization);
        }
        /// <summary>
        /// Reset the culture of all widgets in the cloned content if this is a translation
        /// </summary>
        /// <param name="context"></param>
        /// <param name="part"></param>
        private void ResetWidgetsLocalization(CloneContentContext context, WidgetsContainerPart part) {
            var baseLocPart = part.ContentItem.As<LocalizationPart>();
            if (baseLocPart != null) {
                var routeData = _wca.GetContext().HttpContext.Request.RequestContext.RouteData.Values;
                object action, area;
                if (routeData.TryGetValue("action", out action) &&
                        routeData.TryGetValue("area", out area) &&
                        action.ToString().ToUpperInvariant() == "TRANSLATE" &&
                        area.ToString().ToUpperInvariant() == "ORCHARD.LOCALIZATION") {
                    var widgetsLocParts = _widgetManager
                        .GetWidgets(context.CloneContentItem.Id, context.CloneContentItem.IsPublished())
                        .Select(wi => wi.ContentItem.As<LocalizationPart>())
                        .Where(pa => pa != null);
                    foreach (var wLocPart in widgetsLocParts) {
                        wLocPart.Culture = null;
                    }
                }
            }
        }
    }
}