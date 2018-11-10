using Orchard.ContentManagement;
using System.Collections.Generic;

namespace Laser.Orchard.Reporting.ViewModels {
    public class ShowDashboardViewModel {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<ContentItem> Reports { get; set; }
        public ContentItem Filters { get; set; }
        public ShowDashboardViewModel() {
            Reports = new List<ContentItem>();
        }
    }
}