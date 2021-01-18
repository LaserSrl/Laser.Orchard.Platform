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
                    .Column<string>("ApiKey")
                    .Column<string>("ApiSecret")
                    .Column<string>("ApiSecretHash")
                    .Column<string>("HashAlgorithm")
                    .Column<string>("SecretSalt")
                    .Column<DateTime>("CreatedUtc")
                    .Column<DateTime>("LastLoginUtc")
                );
            return 1;
        }
        

    }
}