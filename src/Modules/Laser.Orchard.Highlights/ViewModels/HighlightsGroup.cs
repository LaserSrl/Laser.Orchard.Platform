using System.Collections.Generic;
using Laser.Orchard.Highlights.Enums;
using Laser.Orchard.Highlights.Models;

using System.Linq;
using Orchard.ContentManagement;
using Orchard.Projections.ViewModels;
//using Orchard.Core.Navigation.Services;

namespace Laser.Orchard.Highlights.ViewModels {

    public class HighlightsGroup {
        public HighlightsGroup() {
            HighlightsItems = Enumerable.Empty<HighlightsItemPart>().ToList();
        }

        public int Id { get; set; }
        public int ContentId { get; set; }
        public string DisplayPlugin { get; set; }
        public DisplayTemplate DisplayTemplate { get; set; }
        public IList<HighlightsItemPart> HighlightsItems { get; set; }
        public string HighlightsItemsOrder { get; set; }

        public IEnumerable<IContent> Highlightss { get; set; }
        public IContent CurrentBanner { get; set; }

        public ItemsSourceTypes ItemsSourceType { get; set; }
        public int Query_Id { get; set; }
        public IEnumerable<QueryRecordEntry> QueryRecordEntries { get; set; }
    }
}