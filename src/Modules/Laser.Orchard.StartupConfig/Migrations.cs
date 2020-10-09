using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Title.Models;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Orchard.Projections.Models;
using System.Data;

namespace Laser.Orchard.StartupConfig {

    public class MigrationStartupConfig : DataMigrationImpl {

        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("CommonPart", b => b
                                                            .WithField("Creator", cfg => cfg.OfType("NumericField").WithDisplayName("Id of user Creator"))
                                                            .WithField("LastModifier", cfg => cfg.OfType("NumericField").WithDisplayName("Id of last user that have modified the content item"))
                                                            );
            ContentDefinitionManager.AlterPartDefinition("PublishExtensionPart", b => b
                .Attachable()
                .WithField("PublishExtensionStatus", cfg => cfg.OfType("EnumerationField")
                    .WithSetting("EnumerationFieldSettings.Required", "true")
                    .WithSetting("EnumerationFieldSettings.ListMode", "Dropdown")
                    .WithSetting("EnumerationFieldSettings.Options", "Created\r\nLoaded\r\nAccepted\r\nRejected")
                    .WithDisplayName("Status"))

                );
            SchemaBuilder.CreateTable("FavoriteCulturePartRecord", table =>
                    table.ContentPartVersionRecord()
                    .Column<int>("Culture_Id", cfg => cfg.Nullable())
                    );
            SchemaBuilder.AlterTable("FavoriteCulturePartRecord", table =>
               table.CreateIndex("CultureIdIndex", "Culture_Id"));
            ContentDefinitionManager.AlterPartDefinition("FavoriteCulturePart", part =>
                part.Attachable(false));
            ContentDefinitionManager.AlterPartDefinition("AuthenticationRequiredPart", b => b
                .Attachable()
            );
            ContentDefinitionManager.AlterTypeDefinition("AuthenticatedProjection", cfg => cfg
                .WithPart("CommonPart")
                .WithPart(typeof(TitlePart).Name)
                .WithPart(typeof(AutoroutePart).Name)
                .WithPart(typeof(MenuPart).Name)
                .WithPart(typeof(ProjectionPart).Name)
                .WithPart(typeof(AdminMenuPart).Name)
                .WithPart(typeof(AuthenticationRequiredPart).Name)
                .Creatable(true)
                .Draftable(false)
                .Listable()
                );
            ContentDefinitionManager.AlterPartDefinition("DisplayTextPart", p => p.Attachable(true));
            ContentDefinitionManager.AlterPartDefinition("ScheduledTaskParametersPart", part => part
                .WithField("Parameters", cfg => cfg.OfType("TextField")));
            ContentDefinitionManager.AlterTypeDefinition("ScheduledTaskParameters", cfg => cfg
                .WithPart("ScheduledTaskParametersPart")
                .Creatable(false)
                .Draftable(false)
                .Listable(false)
                .Securable(false));
            ContentDefinitionManager.AlterPartDefinition("HeaderFooterWidget",
                p => p
                    .WithField("HtmlHeader", cfg => cfg.OfType("TextField")
                        .WithDisplayName("Html Header")
                        .WithSetting("TextFieldSettings.Flavor", "Textarea"))
                    .WithField("HtmlFooter", cfg => cfg.OfType("TextField")
                        .WithDisplayName("Html Footer")
                        .WithSetting("TextFieldSettings.Flavor", "Textarea")));

            ContentDefinitionManager.AlterTypeDefinition("HeaderFooterWidget",
            cfg => cfg
                .WithPart("CommonPart")
                .WithPart("HeaderFooterWidget")
                .WithPart("WidgetPart")
                .WithSetting("Stereotype", "Widget")
            );
            ContentDefinitionManager.AlterPartDefinition("EnsureAsFrontEndInvisiblePart",
                p => p.Attachable(true));

            return 10;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("PublishExtensionPart", b => b
                .Attachable()
                .WithField("PublishExtensionStatus", cfg => cfg.OfType("EnumerationField")
                    .WithSetting("EnumerationFieldSettings.Required", "true")
                    .WithSetting("EnumerationFieldSettings.ListMode", "Dropdown")
                    .WithSetting("EnumerationFieldSettings.Options", "Created\r\nLoaded\r\nAccepted\r\nRejected")
                    .WithDisplayName("Status"))

                );

            return 2;
        }
        public int UpdateFrom2() {
            SchemaBuilder.CreateTable("FavoriteCulturePartRecord", table =>
                    table.ContentPartVersionRecord()
                    .Column<int>("Culture_Id", cfg => cfg.Nullable())
                    );
            SchemaBuilder.AlterTable("FavoriteCulturePartRecord", table =>
               table.CreateIndex("CultureIdIndex", "Culture_Id"));
            ContentDefinitionManager.AlterPartDefinition("FavoriteCulturePart", part =>
                part.Attachable(false));
            return 3;
        }
        public int UpdateFrom3() {
            ContentDefinitionManager.AlterPartDefinition("AuthenticationRequiredPart", b => b
                .Attachable()
            );
            return 4;
        }
        public int UpdateFrom4() {
            ContentDefinitionManager.AlterTypeDefinition("AuthenticatedProjection", cfg => cfg
                .WithPart("CommonPart")
                .WithPart(typeof(TitlePart).Name)
                .WithPart(typeof(AutoroutePart).Name)
                .WithPart(typeof(MenuPart).Name)
                .WithPart(typeof(ProjectionPart).Name)
                .WithPart(typeof(AdminMenuPart).Name)
                .WithPart(typeof(AuthenticationRequiredPart).Name)
                .Creatable(true)
                .Draftable(false)
                );
            return 5;
        }
        public int UpdateFrom5() {
            ContentDefinitionManager.AlterPartDefinition("DisplayTextPart", p => p.Attachable(true));
            return 6;
        }

        public int UpdateFrom6() {
            ContentDefinitionManager.AlterTypeDefinition("AuthenticatedProjection", type => type.Listable());
            return 7;
        }
        public int UpdateFrom7() {
            ContentDefinitionManager.AlterPartDefinition("ScheduledTaskParametersPart", part => part
                .WithField("Parameters", cfg => cfg.OfType("TextField")));
            ContentDefinitionManager.AlterTypeDefinition("ScheduledTaskParameters", cfg => cfg
                .WithPart("ScheduledTaskParametersPart")
                .Creatable(false)
                .Draftable(false)
                .Listable(false)
                .Securable(false));
            return 8;
        }
        public int UpdateFrom8() {
            ContentDefinitionManager.AlterPartDefinition("HeaderFooterWidget",
                p => p
                    .WithField("HtmlHeader", cfg => cfg.OfType("TextField")
                        .WithDisplayName("Html Header")
                        .WithSetting("TextFieldSettings.Flavor", "Textarea"))
                    .WithField("HtmlFooter", cfg => cfg.OfType("TextField")
                        .WithDisplayName("Html Footer")
                        .WithSetting("TextFieldSettings.Flavor", "Textarea")));

            ContentDefinitionManager.AlterTypeDefinition("HeaderFooterWidget",
            cfg => cfg
                .WithPart("CommonPart")
                .WithPart("HeaderFooterWidget")
                .WithPart("WidgetPart")
                .WithSetting("Stereotype", "Widget")
            );
            return 9;
        }
        public int UpdateFrom9() {
            ContentDefinitionManager.AlterPartDefinition("EnsureAsFrontEndInvisiblePart",
                                                    p => p.Attachable(true));
            return 10;
        }
    }

    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class MigrationsUsersGroups : DataMigrationImpl {

        public int Create() {
            return 1;
        }

        public int UpdateFrom1() {
            //ContentDefinitionManager.AlterPartDefinition("UsersGroupsPart", part => part.Attachable());
            ContentDefinitionManager.AlterTypeDefinition("User",
                cfg => cfg
                    .WithPart(typeof(UsersGroupsPart).Name)
                );
            //SchemaBuilder.CreateTable("UsersGroupsSettingsPartRecord", table => table
            //           .ContentPartRecord()
            //           .Column<string>("GroupSerialized")
            //);

            SchemaBuilder.CreateTable("UsersGroupsPartRecord", table => table
                       .ContentPartRecord()
                       .Column<string>("theUserGroup")
           );

            SchemaBuilder.CreateTable("ExtendedUsersGroupsRecord",
        table =>

                table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("GroupName", column => column.WithLength(50))
                );
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("ExtendedUsersGroupsRecord", table => table.AlterColumn("GroupName", col => col.WithType(System.Data.DbType.String).WithLength(150)));
            return 3;
        }


    }

    /// <summary>
    /// Estendo le taxonomy con una informazione in grado di modificare l'ordinamento dei risultati
    /// </summary>
    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
    public class MigrationTaxonomies : DataMigrationImpl {

        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("Taxonomy",
            cfg => cfg
                .WithPart(typeof(TaxonomyExtensionPart).Name)
            );
            return 1;
        }
    }

    [OrchardFeature("Laser.Orchard.StartupConfig.Maintenance")]
    public class MigrationMaintenance : DataMigrationImpl {
        private readonly IUtilsServices _utilsServices;

        public MigrationMaintenance(IUtilsServices utilsServices) {
            _utilsServices = utilsServices;
        }

        public int Create() {
            _utilsServices.EnableFeature("Orchard.PublishLater");
            ContentDefinitionManager.AlterPartDefinition("MaintenancePart", b => b
                .Attachable(false));
            ContentDefinitionManager.AlterTypeDefinition("Maintenance", cfg => cfg
                .WithPart("CommonPart")
                .WithPart(typeof(MaintenancePart).Name)
                .WithPart("PublishLaterPart")
                .Creatable(false)
                .Draftable()
                );
            SchemaBuilder.CreateTable("MaintenancePartRecord", table => table
                .ContentPartVersionRecord()
                .Column<string>("MaintenanceNotifyType")
                .Column<string>("MaintenanceNotify")
            );
            SchemaBuilder.AlterTable("MaintenancePartRecord", table => table
            .AddColumn("Selected_Tenant", DbType.String));
            return 1;
        }
    }
    [OrchardFeature("Laser.Orchard.StartupConfig.JsonDataTablePart")]
    public class MigrationJsonDataTable : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("JsonDataTablePartRecord", table => table
                .ContentPartVersionRecord()
                .Column<string>("TableData", c => c.Nullable().Unlimited())
            );
            ContentDefinitionManager.AlterPartDefinition("JsonDataTablePart", b => b
                .Attachable(true));
            return 1;
        }
    }

    [OrchardFeature("Laser.Orchard.StartupConfig.PerItemCache")]
    public class MigrationPerItemCache : DataMigrationImpl {
        public int Create() {
                SchemaBuilder.CreateTable("PerItemCachePartRecord",
                 table => table
                    .ContentPartVersionRecord()
                    .Column<string>("PerItemKeyParam", column => column.WithLength(1024))
                );

                ContentDefinitionManager.AlterPartDefinition("PerItemCachePart", p => p.Attachable(true));
            return 1;
        }
    }



}