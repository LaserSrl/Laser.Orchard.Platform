using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Laser.Orchard.CulturePicker.Models;
using Orchard.Data.Migration;
using System.Collections.Generic;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.CulturePicker {
    [OrchardFeature("Laser.Orchard.CulturePicker.TranslateMenuItems")]
    public class TranslateMenuItemPartMigrations : DataMigrationImpl {
        
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition(
                typeof(TranslateMenuItemsPart).Name,
                cfg => cfg.Attachable());

            SchemaBuilder.CreateTable(
                "TranslateMenuItemsPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<bool>("ToBeTranslated", column => column.WithDefault(true))
                    .Column<bool>("Translated", column => column.WithDefault(false))
                    .Column<string>("FromLocale", column => column.WithLength(5))
                    );

            ContentDefinitionManager.AlterTypeDefinition("Menu", cfg => cfg
                .WithPart("LocalizationPart")
                .WithPart("TranslateMenuItemsPart")
                );
            
            return 1;
        }


    }
}