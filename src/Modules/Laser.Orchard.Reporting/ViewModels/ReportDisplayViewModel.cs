using Orchard.ContentManagement;

namespace Laser.Orchard.Reporting.ViewModels {
    public class ReportDisplayViewModel {
        public int Id { get; set; }
        public ContentItem FilterContent { get; set; }
        public ContentItem ViewerContent { get; set; }
    }
}