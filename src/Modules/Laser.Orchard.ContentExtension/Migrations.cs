using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Features;

namespace Laser.Orchard.ContentExtension {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("ContentTypePermissionRecord", table => table
                     .Column<int>("Id", column => column.PrimaryKey().Identity())
                     .Column<string>("ContentType")
                     .Column<string>("PostPermission")
                     .Column<string>("GetPermission")
                     .Column<string>("DeletePermission")
                     .Column<string>("PublishPermission")
                  );
            return 1;
        }
        public int UpdateFrom1() {
            return 3;
        }
        public int UpdateFrom2() {
            SchemaBuilder.DropTable("DynamicProjectionPartRecord");
            return 3;
        }
    }

    [OrchardFeature("Laser.Orchard.ContentExtension.DynamicProjection")]
    public class DynamicProjectionMigration : DataMigrationImpl {
        private readonly IFeatureManager _featureManager;

        public DynamicProjectionMigration(IFeatureManager featureManager) {
            _featureManager = featureManager;
        }
        public int Create() {
            SchemaBuilder.CreateTable("DynamicProjectionPartRecord", table => table
                   .ContentPartRecord()
                   .Column<string>("AdminMenuText")
                   .Column<string>("AdminMenuPosition")
                   .Column<bool>("OnAdminMenu", col => col.WithDefault(true))
                   .Column<string>("Icon")
                   .Column<string>("Shape")
                   .Column<string>("ShapeForResults")
                   .Column<int>("Items")
                   .Column<int>("ItemsPerPage")
                   .Column<int>("Skip")
                   .Column<string>("PagerSuffix", col => col.WithLength(255))
                   .Column<bool>("DisplayPager", col => col.WithDefault(true))
                   .Column<int>("MaxItems")
                   .Column<int>("QueryPartRecord_id")
                   .Column<int>("LayoutRecord_Id")
                   .Column<bool>("ReturnsHqlResults")
                   .Column<string>("TypeForFilterForm")
                );
            ContentDefinitionManager.AlterPartDefinition("DynamicProjectionPart", builder => builder
                  .Attachable()
                  .WithDescription("Adds a menu item to the Admin menu that links to this content item."));

            ContentDefinitionManager.AlterTypeDefinition(
                "DynamicProjection",
                type => type
                    .WithPart("DynamicProjectionPart")
                    .WithPart("CommonPart")
                    .WithPart("QueryPickerPart", p => p
                        .WithSetting("QueryPickerPartSettings.IsForHqlQueries", bool.TrueString))
                    .Creatable()
                    .Listable()
                    .WithIdentity()
                );
            return 2;
        }

        public int UpdateFrom1() {

            _featureManager.EnableFeatures(new string[] { "Laser.Orchard.Queries" }, true);
            SchemaBuilder.AlterTable("DynamicProjectionPartRecord", table => table
            .AddColumn<bool>("ReturnsHqlResults")
            );
            SchemaBuilder.AlterTable("DynamicProjectionPartRecord", table => table
            .AddColumn<string>("TypeForFilterForm")
            );
            SchemaBuilder.AlterTable("DynamicProjectionPartRecord", table => table
            .AddColumn<string>("ShapeForResults")
            );
            ContentDefinitionManager.AlterTypeDefinition(
                "DynamicProjection",
                type => type
                    .WithPart("QueryPickerPart", p => p
                        .WithSetting("QueryPickerPartSettings.IsForHqlQueries", bool.TrueString))
                    .WithIdentity()
                );
            return 2;
        }
    }

}