using Orchard.ContentManagement.MetaData.Builders;

namespace Laser.Orchard.Reporting.Settings {
    public class DataReportViewerPartSettings
    {
        public int? DefaultReportId { get; set; }

        public virtual string ContainerTagCssClass { get; set; }

        public virtual string ChartTagCssClass { get; set; }

        public void Build(ContentTypePartDefinitionBuilder builder) {
            builder.WithSetting("DataReportViewerPartSettings.DefaultReportId", DefaultReportId.HasValue ? DefaultReportId.ToString() : string.Empty);
            builder.WithSetting("DataReportViewerPartSettings.ContainerTagCssClass", ContainerTagCssClass);
            builder.WithSetting("DataReportViewerPartSettings.ChartTagCssClass", ChartTagCssClass);
        }
    }
}