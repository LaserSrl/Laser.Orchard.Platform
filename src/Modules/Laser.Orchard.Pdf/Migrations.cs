using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;

namespace Laser.Orchard.Pdf {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("PdfButtonPart", p => p
                .Attachable()
                .WithDescription("Adds a Pdf Button which opens a new window.")
            );
            return 1;
        }
    }
}