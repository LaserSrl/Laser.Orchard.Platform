using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.RazorScripting {

    public class Migrations : DataMigrationImpl {

        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("RazorValidationPart", cfg => cfg
                .Attachable()
                .WithDescription("Provides a way to validate content items using C#.")
                );
            return 1;
        }
    }
}