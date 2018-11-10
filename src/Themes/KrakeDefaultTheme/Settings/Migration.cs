using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KrakeDefaultTheme.Settings {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("ThemeSettingsRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("HeaderLogoUrl", c => c.WithLength(500))
                .Column<string>("PlaceholderLogoUrl", c => c.WithLength(500))
                .Column<string>("BaseLineText", c => c.WithLength(120))
            );

            return 1;
        }
    }
}