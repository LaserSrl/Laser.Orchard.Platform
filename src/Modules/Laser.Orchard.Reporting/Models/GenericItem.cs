using Orchard.ContentManagement;

namespace Laser.Orchard.Reporting.Models {
    public class GenericItem {
        public int Id { get; set; }
        public string Title { get; set; }
        public ContentItem ContentItem { get; set; }
    }
}