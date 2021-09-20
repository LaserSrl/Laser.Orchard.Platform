using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdvancedSettings {
    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class ThemeSkinsMigrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("ThemeSkinsPart", part => part.Attachable());
            return 1;
        }
    }
}