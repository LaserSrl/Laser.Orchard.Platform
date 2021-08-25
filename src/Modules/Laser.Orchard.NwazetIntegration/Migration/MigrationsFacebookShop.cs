using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.NwazetIntegration.Migration {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class MigrationsFacebookShop : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("FacebookShopProductPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("SynchronizeFacebookShop")
            );

            ContentDefinitionManager.AlterPartDefinition(
                "FacebookShopProductPart",
                b => b
                    .Attachable(true)
                    .WithDescription("Provides the synchronization of a product on Facebook Shop")
            );
            return 1;
        }
    }
}