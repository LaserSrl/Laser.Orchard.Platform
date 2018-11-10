using Laser.Orchard.Reporting.Models;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Laser.Orchard.Reporting.ViewModels {
    public class EditDataReportViewerViewModel
    {
        private Collection<SelectListItem> reports = new Collection<SelectListItem>();

        public Collection<SelectListItem> Reports
        {
            get
            {
                return this.reports;
            }
        }

        [Required]
        public int? ReportId { get; set; }

        [Required]
        public ChartTypes ChartType { get; set; }

        public string ContainerCssClass { get; set; }
        public string ChartTagCssClass { get; set; }
        public ColorStyleValues ColorStyle { get; set; }
        public ChartColorNames StartColor { get; set; }
    }
}