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
                    .Column<string>("DeveloperDomainText", col => col.Nullable().Unlimited())
                    .Column<bool>("EnableDeveloperDomain", col => col.WithDefault(false))
                    .Column<string>("GoogleFileContent", col => col.Nullable().Unlimited())
                    .Column<bool>("GoogleEnable", col => col.WithDefault(false))
                );

            return 3;
        }
        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("ManifestAppFileRecord",
                table => table.AddColumn<string>("DeveloperDomainText", col => col.Unlimited().Nullable()));        //.AddColumn<string>("DeveloperDomainText", col => col.Nullable().WithLength(10000)));
            SchemaBuilder.AlterTable("ManifestAppFileRecord",
                table => table.AddColumn<bool>("EnableDeveloperDomain", col => col.WithDefault(false)));
            return 2;
        }
        public int UpdateFrom2()
        {
            SchemaBuilder.AlterTable("ManifestAppFileRecord",
                table => table.AddColumn<string>("GoogleFileContent", col => col.Unlimited().Nullable()));        
            SchemaBuilder.AlterTable("ManifestAppFileRecord",
                table => table.AddColumn<bool>("GoogleEnable", col => col.WithDefault(false)));
            return 3;
        }
    }
}