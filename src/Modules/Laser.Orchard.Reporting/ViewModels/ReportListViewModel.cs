using System.Collections.ObjectModel;

namespace Laser.Orchard.Reporting.ViewModels {
    public class ReportListViewModel
    {
        private Collection<ReportViewModel> reports = new Collection<ReportViewModel>();

        public Collection<ReportViewModel> Reports
        {
            get { return this.reports; }
        }

        public dynamic Pager { get; set; }
    }
}