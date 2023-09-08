using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Sharing
{
    public class SharingDataMigration : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterPartDefinition("ShareBarPart", builder => builder.Attachable());
            
            return 2;
        }

        public int UpdateFrom1() {
            SchemaBuilder.DropTable("ShareBarSettingsPartRecord");
            return 2;
        }
    }
}