using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;

namespace Laser.Orchard.StartupConfig.AppleEnvironment.Migrations {
    [OrchardFeature("Laser.Orchard.ApplePay.DomainAssociation")]
    public class MerchantDomainAssociationMigration : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("MerchantDomainAssociationRecord",
                table => table
                    .Column<int>("Id", col => col.PrimaryKey().Identity())
                    .Column<string>("FileContent", col => col.Nullable().Unlimited())
                    .Column<bool>("Enable", col => col.WithDefault(false))
                );

            return 1;
        }
    }
}