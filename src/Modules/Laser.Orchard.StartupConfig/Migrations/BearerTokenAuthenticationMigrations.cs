using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;

namespace Laser.Orchard.StartupConfig.Migrations {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class BearerTokenAuthenticationMigrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("ApiCredentialsPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("Key")
                    .Column<string>("Secret")
                    .Column<DateTime>("CreatedUtc")
                    .Column<DateTime>("LastLoginUtc")
                );
            return 1;
        }

    }
}