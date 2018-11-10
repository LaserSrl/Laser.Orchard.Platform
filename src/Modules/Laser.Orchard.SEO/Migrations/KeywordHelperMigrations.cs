using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;

namespace Laser.Orchard.SEO {
    [OrchardFeature("Laser.Orchard.KeywordHelper")]
    public class KeywordHelperMigrations : DataMigrationImpl {

        public int Create() {

            SchemaBuilder
                .CreateTable("KeywordHelperPartVersionRecord",
                    table => table
                        .ContentPartVersionRecord()
                        .Column<string>("Keywords")
                        );

            ContentDefinitionManager
                .AlterPartDefinition("KeywordHelperPart",
                    cfg => cfg
                        .Attachable()
                        .WithDescription("Allows content creators to generate recommended keywords to use.")
                        );
            return 1;
        }
    }
}