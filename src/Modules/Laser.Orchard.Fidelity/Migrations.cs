using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;

namespace Laser.Orchard.Fidelity
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterTypeDefinition(
                "User",
                type => type
                            .WithPart("LoyalzooUserPart")
            );

            return 1;
        }
    }
}