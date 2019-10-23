using Orchard.Data.Migration;

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
    }
}