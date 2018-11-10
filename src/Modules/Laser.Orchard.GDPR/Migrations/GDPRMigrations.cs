using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.GDPR.Migrations {
    public class GDPRMigrations : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("GDPRPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("IsProtected"));

            ContentDefinitionManager.AlterPartDefinition("GDPRPart", builder => builder
                .Attachable()

                .WithDescription("Marks items of a ContentType as containing personal identifiable information and enables handling of such information."));

            return 1;
        }
    }
}