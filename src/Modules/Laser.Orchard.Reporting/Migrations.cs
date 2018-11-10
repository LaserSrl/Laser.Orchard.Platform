using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Schema;

namespace Laser.Orchard.Reporting {
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            // Create ReportRecord table
            SchemaBuilder.CreateTable("ReportRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<string>("Title", c => c.WithLength(100).NotNull())
                .Column<string>("Name", c => c.WithLength(100).NotNull())
                .Column<int>("Query_Id", c => c.NotNull())
                .Column<int>("ChartType", c => c.NotNull())
                .Column<int>("AggregateMethod", c => c.NotNull())
                .Column<string>("State", c => c.Unlimited())
                .Column<string>("GroupByCategory", c => c.WithLength(100).NotNull())
                .Column<string>("GroupByType", c => c.WithLength(100).NotNull()));

            // Create DataReportViewerPartRecord table
            SchemaBuilder.CreateTable("DataReportViewerPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("Report_Id", c => c.Nullable())
                .Column<string>("ContainerTagCssClass", c=>c.Nullable().WithLength(100))
                .Column<string>("ChartTagCssClass", c=>c.Nullable().WithLength(100)));

            ContentDefinitionManager.AlterPartDefinition("DataReportViewerPart", builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition("DataReportViewer", cfg => cfg
              .WithPart("CommonPart")
              .WithPart("IdentityPart")
              .WithPart("TitlePart")
              .WithPart("DataReportViewerPart")
              .Creatable()
              .Listable()
              .DisplayedAs("Data Report Viewer"));

            ContentDefinitionManager.AlterTypeDefinition("DataReportViewerWidget", cfg => cfg
              .WithPart("CommonPart")
              .WithPart("IdentityPart")
              .WithPart("DataReportViewerPart")
              .WithPart("WidgetPart")
              .WithSetting("Stereotype", "Widget")
              .DisplayedAs("Data Report Viewer Widget"));

            return 1;
        }
        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("DataReportDashboardPart", p => p
                .Attachable(false)
                .WithField("ReportIds", f => f
                    .OfType("ContentPickerField")
                    .WithSetting("ContentPickerFieldSettings.Hint", "Select one or more Reports.")
                    .WithSetting("ContentPickerFieldSettings.Required", "True")
                    .WithSetting("ContentPickerFieldSettings.Multiple", "True")
                    .WithSetting("ContentPickerFieldSettings.ShowContentTab", "True")
                    .WithSetting("ContentPickerFieldSettings.ShowSearchTab", "True")
                    .WithSetting("ContentPickerFieldSettings.DisplayedContentTypes", "")
                    .WithDisplayName("Reports included")
                )
            );
            ContentDefinitionManager.AlterTypeDefinition("DataReportDashboard", t => t
                .WithPart("TitlePart")
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("DataReportDashboardPart")
                .Creatable()
                .Listable()
                .Draftable(false)
                .Securable()
            );
            return 2;
        }
        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("DataReportViewerPartRecord", t => t
                .AddColumn<int>("ColorStyle", c => c.NotNull().WithDefault(0))
            );
            SchemaBuilder.AlterTable("DataReportViewerPartRecord", t => t
                .AddColumn<int>("StartColor", c => c.NotNull().WithDefault(0))
            );
            return 3;
        }
        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("DataReportViewerPartRecord", t => t
                .AddColumn<int>("ChartType", c => c.NotNull().WithDefault(0))
            );
            SchemaBuilder.AlterTable("ReportRecord", t => t
                .DropColumn("ChartType")
            );
            return 4;
        }
    }
}