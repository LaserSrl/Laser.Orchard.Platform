using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Laser.Orchard.Reporting.ViewModels {
    public class HqlReportViewModel {
        private Collection<SelectListItem> queries = new Collection<SelectListItem>();
        private Collection<SelectListItem> chartTypes = new Collection<SelectListItem>();

        public Collection<SelectListItem> Queries {
            get {
                return this.queries;
            }
        }

        public Collection<SelectListItem> ChartTypes {
            get {
                return this.chartTypes;
            }
        }

        public int ReportId { get; set; }

        [MaxLength(100)]
        [Required]
        public string Name { get; set; }

        [MaxLength(100)]
        [Required]
        public string Title { get; set; }

        [Required]
        public int ChartTypeId { get; set; }

        [Required]
        public int? QueryId { get; set; }

        public string ColumnAliases { get; set; }
    }
}