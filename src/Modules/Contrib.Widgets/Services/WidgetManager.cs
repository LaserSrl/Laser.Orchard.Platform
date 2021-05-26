using System.Collections.Generic;
using System.Linq;
using Contrib.Widgets.Models;
using Orchard.ContentManagement;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace Contrib.Widgets.Services {
    public class WidgetManager : IWidgetManager {
        private readonly IContentManager _contentManager;
        private readonly IWidgetsService _widgetsService;

        public WidgetManager(IContentManager contentManager, IWidgetsService widgetsService) {
            _contentManager = contentManager;
            _widgetsService = widgetsService;
        }

        public IEnumerable<WidgetExPart> GetWidgets(int hostId) {
            return _contentManager
                .Query<WidgetExPart, WidgetExPartRecord>()
                .Where(x => x.HostId == hostId)
                .List();
        }
        public IEnumerable<WidgetExPart> GetWidgets(int hostId, bool published) {
            return _contentManager
                .Query<WidgetExPart, WidgetExPartRecord>(published ? VersionOptions.Published : VersionOptions.Latest)
                .Where(x => x.HostId == hostId)
                .List();
        }

        public LayerPart GetContentLayer() {
            var contentLayer = _widgetsService.GetLayers().FirstOrDefault(x => x.Name == "ContentWidgets")
                ?? _widgetsService.CreateLayer("ContentWidgets", ContentWidgetLayerDescription(), "false");

            return contentLayer;
        }

        public string ContentWidgetLayerDescription() {
            return "This layer never activates, but is needed for the widgets hosted by content items for now.";
        }
    }
}