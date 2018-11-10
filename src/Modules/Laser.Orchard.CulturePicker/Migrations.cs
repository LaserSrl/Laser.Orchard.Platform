using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Laser.Orchard.CulturePicker.Models;
using Orchard.Data.Migration;
using System.Collections.Generic;

namespace Laser.Orchard.CulturePicker {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            ContentDefinitionManager.AlterPartDefinition(typeof (CulturePickerPart).Name, cfg => cfg
                                                                                                          .Attachable());
            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("CulturePickerWidget", cfg => cfg
                .WithPart("CulturePickerPart")
                .WithPart("WidgetPart")
                .WithPart("CommonPart")
                .WithSetting("Stereotype", "Widget"));

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.CreateTable("ExtendedCultureRecord",
            table =>

                    table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("CultureCode", column => column.WithLength(5))
                    .Column<string>("DisplayName", column => column.WithLength(50))
                    .Column<int>("Priority")
                    );

            SchemaBuilder.CreateTable("SettingsRecord",
            table =>

                    table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<bool>("ShowOnlyPertinentCultures")
                    .Column<bool>("ShowLabel", column => column.WithDefault(true))
                    );
            return 3;
        }

        

    }
}