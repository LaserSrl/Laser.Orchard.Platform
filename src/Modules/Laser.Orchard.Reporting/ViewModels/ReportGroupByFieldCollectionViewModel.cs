using Orchard.Localization;
using System.Collections.ObjectModel;

namespace Laser.Orchard.Reporting.ViewModels {
    public class ReportGroupByFieldCollectionViewModel
    {
        private Collection<ReportGroupByFieldViewModel> groupByFields = new Collection<ReportGroupByFieldViewModel>();
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }

        public Collection<ReportGroupByFieldViewModel> GroupByFields
        {
            get
            {
                return this.groupByFields;
            }
        }
    }
}