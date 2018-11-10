using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using System.Data;

namespace Laser.Orchard.ShareLink {

    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("ShareLinkPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("SharedLink")
                .Column<string>("SharedText")
                .Column<string>("SharedImage")
                );
            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition(
                "ShareLinkPart",
                b => b
                    .Attachable(true)
            );
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.CreateTable("ShareLinkModuleSettingPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("SharedLink")
                .Column<string>("SharedText")
                .Column<string>("SharedImage")
            );
            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("ShareLinkPartRecord", table => table
                .AddColumn<string>("SharedIdImage")
            );
            return 4;
        }
        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("ShareLinkPartRecord", table => table
                .AddColumn<string>("SharedBody")
            );
            SchemaBuilder.AlterTable("ShareLinkModuleSettingPartRecord", table => table
                .AddColumn<string>("SharedBody")
            );
            SchemaBuilder.AlterTable("ShareLinkModuleSettingPartRecord", table => table
                .AddColumn<string>("Fb_App")
            );
            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("ShareLinkPartRecord", table => table
                .AlterColumn("SharedBody", column => column.WithType(DbType.String).Unlimited())
            );
            SchemaBuilder.AlterTable("ShareLinkPartRecord", table => table
                .AlterColumn("SharedLink", column => column.WithType(DbType.String).Unlimited())
            );
            SchemaBuilder.AlterTable("ShareLinkPartRecord", table => table
                .AlterColumn("SharedText", column => column.WithType(DbType.String).Unlimited())
            );
            SchemaBuilder.AlterTable("ShareLinkPartRecord", table => table
                .AlterColumn("SharedImage", column => column.WithType(DbType.String).WithLength(2048))
            );
            SchemaBuilder.AlterTable("ShareLinkPartRecord", table => table
                .AlterColumn("SharedIdImage", column => column.WithType(DbType.String).Unlimited())
            );

            return 6;
        }

    }
}