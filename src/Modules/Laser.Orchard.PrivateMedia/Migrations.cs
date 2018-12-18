using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.PrivateMedia {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("PrivateMediaPartRecord",
               table => table
                   .ContentPartRecord()
                   .Column<bool>("IsPrivate", col => col.WithDefault(false))
            );
            ContentDefinitionManager.AlterPartDefinition(
                "PrivateMediaPart",
                 b => b
                    .Attachable(false)
            );
            ContentDefinitionManager.AlterTypeDefinition("Image",
                t => t.WithPart("PrivateMediaPart")
            );
            return 1;
        }
    }
}