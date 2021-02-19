using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Migrations {
    [OrchardFeature("Laser.Orchard.StartupConfig.FrontendEditorConfiguration")]
    public class FrontendEditorConfigurationMigrations : DataMigrationImpl {

        public int Create() {
            ContentDefinitionManager.AlterPartDefinition(
                typeof(FrontendEditConfigurationPart).Name,
                cfg =>
                    cfg.Attachable());
            return 1;
        }
    }
}