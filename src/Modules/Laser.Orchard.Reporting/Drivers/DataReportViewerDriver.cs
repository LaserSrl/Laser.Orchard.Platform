using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Projections.Services;
using Laser.Orchard.Reporting.Models;
using Laser.Orchard.Reporting.Services;
using Laser.Orchard.Reporting.Settings;
using Laser.Orchard.Reporting.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Laser.Orchard.Reporting.Providers;
using Orchard.ContentManagement.Handlers;
using System.Xml.Linq;
using Orchard.Projections.Models;

namespace Laser.Orchard.Reporting.Drivers {
    public class DataReportViewerDriver : ContentPartDriver<DataReportViewerPart>
    {
        private readonly IReportManager reportManger;
        private readonly IRepository<ReportRecord> reportRepository;
        private readonly IProjectionManager projectionManager;
        private readonly IContentManager _contentManager;

        public DataReportViewerDriver(
            IReportManager reportManger,
            IProjectionManager projectionManager,
            IRepository<ReportRecord> reportRepository,
            IContentManager contentManager)
        {
            this.projectionManager = projectionManager;
            this.reportRepository = reportRepository;
            this.reportManger = reportManger;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T;

        protected override DriverResult Display(DataReportViewerPart part, string displayType, dynamic shapeHelper)
        {
            if (part.Record.Report == null)
            {
                return null;
            }

            if (displayType == "Detail")
            {
                var report = this.reportRepository.Table.FirstOrDefault(c => c.Id == part.Record.Report.Id);
                var partSettings = part.Settings.GetModel<DataReportViewerPartSettings>();

                if (report == null)
                {
                    return null;
                }

                IEnumerable<AggregationResult> reportData = null;
                int count = 0;
                if(string.IsNullOrWhiteSpace(report.GroupByCategory)) {
                    var multiColumn = part.Record.ChartType == ((int)ChartTypes.SimpleTable);
                    reportData = reportManger.RunHqlReport(report, part.ContentItem, multiColumn);
                    count = reportData.Count();
                } else {
                    reportData = reportManger.RunReport(report, part.ContentItem);
                    count = reportManger.GetCount(report, part.ContentItem);
                }

                // colors
                List<string> colors = new List<string>();
                string[] palette = ColorsSettings.ChartColors;
                if(part.Record.ColorStyle == (int)ColorStyleValues.Light) {
                    palette = ColorsSettings.ChartColorsLight;
                }
                else if (part.Record.ColorStyle == (int)ColorStyleValues.Dark) {
                    palette = ColorsSettings.ChartColorsDark;
                }
                if (part.Record.StartColor == 0) {
                    colors.AddRange(palette);
                } else {
                    colors.AddRange(palette.Skip(part.Record.StartColor));
                    colors.AddRange(palette.Take(part.Record.StartColor));
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                var model = new DataReportViewerViewModel
                {
                    TotalCount = count,
                    ReportTitle = part.Record.Report.Title,
                    JsonData = serializer.Serialize(reportData.Select(c => new object[] { c.Label, c.AggregationValue }).ToArray()),
                    Data = reportData.ToList(),
                    ChartCssClass = partSettings.ChartTagCssClass,
                    ContainerCssClass = partSettings.ContainerTagCssClass,
                    ColorsArray = serializer.Serialize(colors),
                    HtmlId = part.Record.Id,
                    AdditionalChartWidth = Convert.ToInt32(Math.Floor(reportData.Max(x => x.Label.Length) * 0.33)) //0.33: empirical value to adjust chart width
                };

                if (part.Record.ChartType == (int)ChartTypes.PieChart)
                {
                    return ContentShape("Parts_DataReportViewer_PieChart",
                         () => shapeHelper.Parts_DataReportViewer_PieChart(Model: model));
                }
                else if (part.Record.ChartType == (int)ChartTypes.SimpleList)
                {
                    return ContentShape("Parts_DataReportViewer_SimpleList",
                         () => shapeHelper.Parts_DataReportViewer_SimpleList(Model: model));
                } else if (part.Record.ChartType == (int)ChartTypes.Donut) {
                    return ContentShape("Parts_DataReportViewer_Donut",
                         () => shapeHelper.Parts_DataReportViewer_Donut(Model: model));
                } else if (part.Record.ChartType == (int)ChartTypes.Histogram) {
                    return ContentShape("Parts_DataReportViewer_Histogram",
                         () => shapeHelper.Parts_DataReportViewer_Histogram(Model: model));
                } else if (part.Record.ChartType == (int)ChartTypes.LineChart) {
                    return ContentShape("Parts_DataReportViewer_LineChart",
                         () => shapeHelper.Parts_DataReportViewer_LineChart(Model: model));
                } else if (part.Record.ChartType == (int)ChartTypes.HorizontalBars) {
                    return ContentShape("Parts_DataReportViewer_HorizontalBars",
                         () => shapeHelper.Parts_DataReportViewer_HorizontalBars(Model: model));
                } else if (part.Record.ChartType == (int)ChartTypes.SimpleTable) {
                    return ContentShape("Parts_DataReportViewer_SimpleTable",
                         () => shapeHelper.Parts_DataReportViewer_SimpleTable(Model: model));
                } else {
                    return ContentShape("Parts_DataReportViewer",
                          () => shapeHelper.Parts_DataReportViewer_Summary(
                              Model: new DataReportViewerViewModel { ReportTitle = part.Record.Report.Title }
                              ));
                }
            }
            else
            {
                return ContentShape("Parts_DataReportViewer",
                     () => shapeHelper.Parts_DataReportViewer_Summary(
                         Model: new DataReportViewerViewModel { ReportTitle = part.Record.Report.Title}
                         ));
            }
        }

        protected override DriverResult Editor(DataReportViewerPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            EditDataReportViewerViewModel model = new EditDataReportViewerViewModel();
            updater.TryUpdateModel(model, "DataReportViewerPart", null, null);

            if (model.ReportId == null)
            {
                updater.AddModelError("ReportId", T("Please select a Report"));
            }
            else
            {
                part.Record.Report = new ReportRecord { Id = model.ReportId.Value };
                part.Record.ChartTagCssClass = model.ChartTagCssClass;
                part.Record.ContainerTagCssClass = model.ContainerCssClass;
                part.Record.ChartType = (int)(model.ChartType);
                part.Record.ColorStyle = (int)(model.ColorStyle);
                part.Record.StartColor = (int)(model.StartColor);
            }

            return this.Editor(part, shapeHelper);
        }

        protected override DriverResult Editor(DataReportViewerPart part, dynamic shapeHelper)
        {
            var model = new EditDataReportViewerViewModel();
            var record = part.Record;

            // if it is called for creating a new item, then the default values from settings must be copied into the part
            if (record.Id == default(int))
            {
                var settings = part.TypePartDefinition.Settings.GetModel<DataReportViewerPartSettings>();
                model.ChartTagCssClass = settings.ChartTagCssClass;
                model.ContainerCssClass = settings.ContainerTagCssClass;
                model.ReportId = settings.DefaultReportId;
            }
            else
            {
                if (record.Report != null)
                {
                    model.ReportId = record.Report.Id;
                }
                model.ChartType = (ChartTypes)(record.ChartType);
                model.ChartTagCssClass = record.ChartTagCssClass;
                model.ContainerCssClass = record.ContainerTagCssClass;
                model.ColorStyle = (ColorStyleValues)(record.ColorStyle);
                model.StartColor = (ChartColorNames)(record.StartColor);
            }

            var reports = this.reportRepository.Table.ToList();
            model.Reports.AddRange(reports.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(CultureInfo.InvariantCulture),
                Text = c.Title,
                Selected = model.ReportId.HasValue ? model.ReportId.Value == c.Id : false
            }));

            return ContentShape("Parts_DataReportViewer_Edit",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: "Parts/DataReportViewer",
                        Model: model,
                        Prefix: Prefix));
        }
        protected override void Exporting(DataReportViewerPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            if(part.Record != null) {
                root.SetAttributeValue("ChartType", part.Record.ChartType);
                root.SetAttributeValue("ContainerTagCssClass", part.Record.ContainerTagCssClass);
                root.SetAttributeValue("ChartTagCssClass", part.Record.ChartTagCssClass);
                root.SetAttributeValue("ColorStyle", part.Record.ColorStyle);
                root.SetAttributeValue("StartColor", part.Record.StartColor);
                XElement report = new XElement("Report");
                var reportRecord = part.Record.Report;
                report.SetAttributeValue("Name", reportRecord.Name ?? "");
                report.SetAttributeValue("Title", reportRecord.Title ?? "");
                report.SetAttributeValue("State", reportRecord.State ?? "");
                report.SetAttributeValue("GroupByCategory", reportRecord.GroupByCategory ?? "");
                report.SetAttributeValue("GroupByType", reportRecord.GroupByType ?? "");
                report.SetAttributeValue("AggregateMethod", reportRecord.AggregateMethod);
                report.SetAttributeValue("ColumnAliases", reportRecord.ColumnAliases ?? "");
                report.SetAttributeValue("GUID", reportRecord.GUID ?? "");
                var query = _contentManager.Get(reportRecord.Query.Id);
                report.SetAttributeValue("QueryId", _contentManager.GetItemMetadata(query).Identity.ToString());
                root.Add(report);
            }
        }
        protected override void Importing(DataReportViewerPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            var chartType = root.Attribute("ChartType");
            if (chartType != null) {
                part.Record.ChartType = Convert.ToInt32(chartType.Value);
            }
            var containerTagCssClass = root.Attribute("ContainerTagCssClass");
            if (containerTagCssClass != null) {
                part.Record.ContainerTagCssClass = containerTagCssClass.Value;
            }
            var chartTagCssClass = root.Attribute("ChartTagCssClass");
            if (chartTagCssClass != null) {
                part.Record.ChartTagCssClass = chartTagCssClass.Value;
            }
            var colorStyle = root.Attribute("ColorStyle");
            if (colorStyle != null) {
                part.Record.ColorStyle = Convert.ToInt32(colorStyle.Value);
            }
            var startColor = root.Attribute("StartColor");
            if (startColor != null) {
                part.Record.StartColor = Convert.ToInt32(startColor.Value);
            }
            var report = root.Element("Report");
            if(report != null) {
                var guid = report.Attribute("GUID")?.Value ?? "";
                ReportRecord reportRecord = null;
                // match report record based on GUID
                bool newReportRecord = false;
                if (!string.IsNullOrWhiteSpace(guid)) {
                    reportRecord = reportRepository.Table.FirstOrDefault(r => r.GUID == guid);
                }
                if (reportRecord == null) {
                    newReportRecord = true;
                    reportRecord = new ReportRecord() {
                        GUID = string.IsNullOrWhiteSpace(guid)
                            ? Guid.NewGuid().ToString()
                            : guid
                    };
                }
                
                var name = report.Attribute("Name");
                if (name != null) {
                    reportRecord.Name = name.Value;
                }
                var title = report.Attribute("Title");
                if (title != null) {
                    reportRecord.Title = title.Value;
                }
                var state = report.Attribute("State");
                if (state != null) {
                    reportRecord.State = string.IsNullOrEmpty(state.Value) ? null : state.Value;
                }
                var groupByCategory = report.Attribute("GroupByCategory");
                if (groupByCategory != null) {
                    reportRecord.GroupByCategory = groupByCategory.Value;
                }
                var groupByType = report.Attribute("GroupByType");
                if (groupByType != null) {
                    reportRecord.GroupByType = groupByType.Value;
                }
                var aggregateMethod = report.Attribute("AggregateMethod");
                if (aggregateMethod != null) {
                    reportRecord.AggregateMethod = Convert.ToInt32(aggregateMethod.Value);
                }
                var columnAliases = report.Attribute("ColumnAliases");
                if (columnAliases != null) {
                    reportRecord.ColumnAliases = columnAliases.Value;
                }
                var queryId = report.Attribute("QueryId");
                if (queryId != null) {
                    var ciQuery = _contentManager.ResolveIdentity(new ContentIdentity(queryId.Value));
                    if (ciQuery != null) {
                        reportRecord.Query = new QueryPartRecord();
                        reportRecord.Query.Id = ciQuery.Id;
                    }
                }
                if (part.Record.Report == null) {
                    part.Record.Report = reportRecord;
                }
                if (newReportRecord) {
                    reportRepository.Create(reportRecord);
                } else {
                    reportRepository.Update(reportRecord);
                }
            }
        }
    }
}