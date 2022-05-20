using Laser.Orchard.StartupConfig.ContentSync.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ContentSync {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("SyncPartRecord", tbl => tbl
            .ContentPartRecord()
            .Column<int>("SyncronizedRef"));

            ContentDefinitionManager
                .AlterPartDefinition(typeof(SyncPart).Name, 
                    part => part.Attachable());
            return 1;
        }
    }
}