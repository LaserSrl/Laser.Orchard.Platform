using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdvancedSettings {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder
                .CreateTable("AdvancedSettingsPartRecord", table => table
                    .ContentPartRecord()
                    .Column<string>("Name", col => col.WithLength(255)))
                .AlterTable("AdvancedSettingsPartRecord", table => table
                    .CreateIndex("IDX_ASPR_Name", "Name"));

            ContentDefinitionManager.AlterPartDefinition("AdvancedSettingsPart", part => part.Attachable());
            return 1;
        }
    }
}