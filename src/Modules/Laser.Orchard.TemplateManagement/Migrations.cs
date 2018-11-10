using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.TemplateManagement {
    [OrchardFeature("Laser.Orchard.TemplateManagement")]
    public class Migrations : DataMigrationImpl {
         public int Create() {
            SchemaBuilder.CreateTable("TemplatePartRecord", table => table
            .ContentPartRecord()
            .Column<string>("Title", c => c.WithLength(256))
            .Column<string>("Subject", c => c.WithLength(256))
            .Column<string>("Text", c => c.Unlimited())
            .Column<int>("LayoutIdSelected", c => c.Nullable())
            .Column<bool>("IsLayout", c => c.NotNull()));

            SchemaBuilder.CreateTable("SiteSettingsPartRecord", table => table
            .ContentPartRecord()
            .Column<string>("DefaultParserIdSelected"));

            ContentDefinitionManager.AlterPartDefinition("TemplatePart", part => part.Attachable());
            ContentDefinitionManager.AlterTypeDefinition("CustomTemplate", type => type
            .WithPart("CommonPart", part => part
            .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "False"))
            .WithPart("TemplatePart")
            .WithPart("IdentityPart") // Identity Part for Import Export capability
            .DisplayedAs("Custom Template")
            .Draftable()
            .Creatable());

             // nella prima creazione è inutile fare gli update 1-3, perché creo già Type e Part con i nomi corretti
             return 4;
         }

         public int UpdateFrom1() {
            SchemaBuilder.CreateTable("TemplatePartRecord", table => table
            .ContentPartRecord()
            .Column<string>("Title", c => c.WithLength(256))
            .Column<string>("Subject", c => c.WithLength(256))
            .Column<string>("Text", c => c.Unlimited())
            .Column<int>("LayoutIdSelected", c => c.Nullable())
            .Column<bool>("IsLayout", c => c.NotNull()));

            SchemaBuilder.CreateTable("SiteSettingsPartRecord", table => table
            .ContentPartRecord()
            .Column<string>("DefaultParserIdSelected"));

            ContentDefinitionManager.AlterPartDefinition("TemplatePart", part => part.Attachable());
            ContentDefinitionManager.AlterTypeDefinition("Template", type => type
            .WithPart("CommonPart", part => part
            .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "False"))
            .WithPart("TemplatePart")
            .DisplayedAs("Manage Template")
            .Draftable()
            .Creatable());

             return 2;
         }
         public int UpdateFrom2() {
             ContentDefinitionManager.AlterTypeDefinition("Template", type => type
             .WithPart("IdentityPart")); // Identity Part for Import Export capability

             return 3;
         }

         public int UpdateFrom3() {
             // Rinomino i contenttype fisicamente sulla tabella in quanto il content type va in conflitto con il Template Orchard Core
             SchemaBuilder.ExecuteSql("UPDATE Orchard_Framework_ContentTypeRecord SET Name='CustomTemplate' WHERE Name='Template'");
             SchemaBuilder.ExecuteSql("UPDATE Settings_ContentTypeDefinitionRecord SET Name='CustomTemplate', DisplayName='Custom Template', Settings=Replace(cast(settings as nvarchar(4000)), 'DisplayName=\"Manage Template\"','DisplayName=\"Custom Template\"') WHERE Name='Template'");
             return 4;
         }

         public int UpdateFrom4() {
             SchemaBuilder.CreateTable("CustomTemplatePickerPartRecord", table => table
.ContentPartRecord()
.Column<int>("TemplateIdSelected", c => c.Nullable()));

             ContentDefinitionManager.AlterPartDefinition("CustomTemplatePickerPart", part => part.Attachable(false));
             return 5;
         }


        public int UpdateFrom5() {
            ContentDefinitionManager.AlterTypeDefinition("CustomTemplate", type => type.Listable());
            return 6;
        }
        public int UpdateFrom6() {
            return 7;
        }
        public int UpdateFrom7() {
            ContentDefinitionManager.AlterTypeDefinition("Template", type => type.Listable(false));
            return 8;
        }
    }
}