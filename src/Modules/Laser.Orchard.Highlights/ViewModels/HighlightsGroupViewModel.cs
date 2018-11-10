using Laser.Orchard.Highlights.Enums;
using Laser.Orchard.Highlights.Models;
using Orchard.ContentManagement;

namespace Laser.Orchard.Highlights.ViewModels {

    public class HighlightsGroupViewModel {

        public int Id { get; set; }
        public string DisplayPlugin { get; set; }
        public DisplayTemplate DisplayTemplate { get; set; }
        public ItemsSourceTypes ItemsSourceType { get; set; }
    }
}