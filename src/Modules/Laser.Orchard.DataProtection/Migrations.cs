using Orchard.Data.Migration;
using Orchard.Core.Contents.Extensions;
using Orchard.ContentManagement.MetaData;

namespace Laser.Orchard.DataProtection {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("DataProtectionContextPartRecord",
                table => table
                    .ContentPartVersionRecord()
                    .Column("Context", System.Data.DbType.String, c => c.Unlimited()));
            ContentDefinitionManager.AlterPartDefinition(
                "DataProtectionContextPart",
                 b => b
                    .Attachable(true)
            );
            SchemaBuilder.CreateTable("DataRestrictionsPartRecord",
                t => t.ContentPartRecord());
            SchemaBuilder.CreateTable("DataRestrictionsRecord",
                t => t
                    .Column("Id", System.Data.DbType.Int32, c => c.PrimaryKey().Identity())
                    .Column("Restrictions", System.Data.DbType.String, c => c.Unlimited())
                    .Column("DataRestrictionsPartRecord_id", System.Data.DbType.Int32)
                );
            ContentDefinitionManager.AlterPartDefinition(
                "DataRestrictionsPart",
                 b => b
                    .Attachable(false)
            );
            ContentDefinitionManager.AlterTypeDefinition("User",
                t => t.WithPart("DataRestrictionsPart")
            );
            return 1;
        }
    }
}