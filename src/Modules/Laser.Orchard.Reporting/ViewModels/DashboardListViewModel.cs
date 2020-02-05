using Laser.Orchard.Reporting.Models;
using Orchard.UI.Navigation;
using System.Collections.Generic;

namespace Laser.Orchard.Reporting.ViewModels {
    public class DashboardListViewModel {
        public IEnumerable<GenericItem> Dashboards { get; set; }
        public string TitleFilter { get; set; }
        public dynamic Pager { get; set; }
        public PagerParameters PagerParameters { get; set; }
        public int? page { get; set; }
        public int? pageSize { get; set; }
        public string UrlForCreateDashboard { get; set; }
        public DashboardListViewModel() {
            PagerParameters = new PagerParameters();
        }
    }
}