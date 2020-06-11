using Laser.Orchard.Reporting.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Schema;
using Orchard.Roles.Models;
using System;
using System.Linq;

namespace Laser.Orchard.Reporting {
    public class Migrations : DataMigrationImpl {
        private readonly IContentManager _contentManager;
        private readonly IRepository<PermissionRecord> _permissionRepository;
        private readonly IRepository<ReportRecord> _reportRepository;

        public Migrations(
            IContentManager contentManager,
            IRepository<PermissionRecord> permissionRepository,
            IRepository<ReportRecord> reportRepository) {

            _contentManager = contentManager;
            _permissionRepository = permissionRepository;
            _reportRepository = reportRepository;
        }

        public int Create() {
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
                .Column<string>("ContainerTagCssClass", c => c.Nullable().WithLength(100))
                .Column<string>("ChartTagCssClass", c => c.Nullable().WithLength(100)));

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
        public int UpdateFrom4() {
            ContentDefinitionManager.AlterPartDefinition("DataReportDashboardPart", p => p
                .Attachable(false)
                .WithField("ReportIds", f => f
                    .OfType("ContentPickerField")
                    .WithSetting("ContentPickerFieldSettings.Hint", "Select one or more Reports.")
                    .WithSetting("ContentPickerFieldSettings.Required", "True")
                    .WithSetting("ContentPickerFieldSettings.Multiple", "True")
                    .WithSetting("ContentPickerFieldSettings.ShowContentTab", "True")
                    .WithSetting("ContentPickerFieldSettings.ShowSearchTab", "True")
                    .WithSetting("ContentPickerFieldSettings.DisplayedContentTypes", "DataReportViewerPart")
                    .WithDisplayName("Reports included")
                )
            );
            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("ReportRecord", table =>
                table.AddColumn<string>("ColumnAliases"));
            return 6;
        }
        public int UpdateFrom6() {
            // Updating permission Names for Reports and Dashboards because they uses identity instead of id
            var reports = _contentManager.Query().ForType("DataReportViewer").List();
            var dashboards = _contentManager.Query().ForType("DataReportDashboard").List();
            foreach (var report in reports) {
                var permission = _permissionRepository.Table.SingleOrDefault(x => x.Name == string.Format("ShowDataReport{0}", report.Id) && x.FeatureName == "Laser.Orchard.Reporting");
                if (permission != null) {
                    permission.Name = string.Format("ShowDataReport{0}", report.As<IdentityPart>().Identifier);
                    _permissionRepository.Update(permission);
                }
            }
            foreach (var dashboard in dashboards) {
                var permission = _permissionRepository.Table.SingleOrDefault(x => x.Name == string.Format("ShowDashboard{0}", dashboard.Id) && x.FeatureName == "Laser.Orchard.Reporting");
                if (permission != null) {
                    permission.Name = string.Format("ShowDashboard{0}", dashboard.As<IdentityPart>().Identifier);
                    _permissionRepository.Update(permission);
                }
            }
            return 7;
        }

        public int UpdateFrom7() {
            SchemaBuilder.AlterTable("ReportRecord", table =>
                table.AddColumn<string>("GUID"));
            return 8;
        }
        public int UpdateFrom8() {
            foreach (var rr in _reportRepository.Table.Where(r => r.GUID == null || r.GUID == "")) {
                rr.GUID = Guid.NewGuid().ToString();
            }
            return 9;
        }
    }
}