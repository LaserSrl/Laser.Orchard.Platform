using System;
using System.Data;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Laser.Orchard.ShortLinks.Models;


namespace Laser.Orchard.ShortLinks {
    public class Migration: DataMigrationImpl {
        
        public int Create() {
            // Creating table ShortLinksRecord
            SchemaBuilder.CreateTable("ShortLinksRecord", table => table
                .ContentPartRecord()
                .Column<string>("Url"));

            ContentDefinitionManager.AlterPartDefinition(typeof(ShortLinksPart).Name, cfg => cfg
                .Attachable());

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("ShortLinksRecord", table => table.AddColumn("FullLink", DbType.String));            

            return 2;
        }

    }


}