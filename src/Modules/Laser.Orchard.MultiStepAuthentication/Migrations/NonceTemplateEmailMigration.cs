using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.MultiStepAuthentication {
    [OrchardFeature("Laser.Orchard.NonceTemplateEmail")]
    public class NonceTemplateEmailMigration : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("NonceTemplateSettingsPartRecord", table => table
            .ContentPartRecord()
            .Column<int>("TemplateIdSelected", c => c.Nullable()));
            return 1;
        }
    }
}