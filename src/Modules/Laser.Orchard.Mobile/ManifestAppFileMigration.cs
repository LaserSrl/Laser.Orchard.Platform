using Orchard.Data.Migration;
using System;

namespace Laser.Orchard.Mobile {
    public class ManifestAppFileMigration : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("ManifestAppFileRecord",
                table => table
                    .Column<int>("Id", col => col.PrimaryKey().Identity())
                    .Column<string>("FileContent", col => col.Nullable().Unlimited())
                    .Column<bool>("Enable", col => col.WithDefault(false))
                );

            return 1;
        }
        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("ManifestAppFileRecord",
                table => table.AddColumn<string>("DeveloperDomainText", col => col.Unlimited().Nullable()));        //.AddColumn<string>("DeveloperDomainText", col => col.Nullable().WithLength(10000)));
            SchemaBuilder.AlterTable("ManifestAppFileRecord",
                table => table.AddColumn<bool>("EnableDeveloperDomain", col => col.WithDefault(false)));
            return 2;
        }
    }
}