using System.Collections.Generic;
using Laser.Orchard.Highlights.Enums;
using Laser.Orchard.Highlights.Models;
using Laser.Orchard.Highlights.ViewModels;
using Orchard;
using Orchard.ContentManagement;

namespace Laser.Orchard.Highlights.Services {

    public interface IHighlightsService : IDependency {

        IEnumerable<HighlightsItemPart> GetHighlightsItemsByGroupId(int groupId);
        IEnumerable<dynamic> GetHighlightsItemsContentByGroupId(int groupId, DisplayTemplate displayTemplate, string displayPlugin, string settingsShapeName);
        IList<string> GetAvailablePlugins();
        HighlightsItemPart Get(int itemId);
        void UpdateOrder(int itemId, int order);
        void Remove(HighlightsItemPart itemId);
        ViewsInfos ChooseView(dynamic part);
        IList<dynamic> MapContentToHighlightsItemsViewModel(IEnumerable<ContentItem> queryItems, Enums.DisplayTemplate displayTemplate, string displayPlugin, ViewsInfos viewsInfos);
        dynamic MapContentToHighlightsItemViewModel(HighlightsItemPart itemPart);
        dynamic CreateHighlightsItemShape(dynamic item, string displayTemplate, string overrideShapeName = "");
        IList<string> GetDisplayPluginsFor(Enums.DisplayTemplate displayTemplate);
        string GetDisplayPluginsPreviewImage();
    }
}
