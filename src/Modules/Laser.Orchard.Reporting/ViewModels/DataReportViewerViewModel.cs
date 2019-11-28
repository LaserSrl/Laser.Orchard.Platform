using Laser.Orchard.Reporting.Providers;
using System.Collections.Generic;

namespace Laser.Orchard.Reporting.ViewModels {
    public class DataReportViewerViewModel
    {
        public int TotalCount { get; set; }
        public string JsonData { get; set; }
        public string ReportTitle { get; set; }
        public string ContainerCssClass { get; set; }
        public string ChartCssClass { get; set; }
        public string ColorsArray { get; set; }
        public List<AggregationResult> Data { get; set; }
        public int HtmlId { get; set; }
        /// <summary>
        /// Additional width for chart expressed in em units to compensate legend width.
        /// </summary>
        public int AdditionalChartWidth { get; set; }
    }
}