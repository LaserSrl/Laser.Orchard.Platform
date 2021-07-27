using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Core.Settings.Metadata;
using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ZoneAlternates.Migrations {
    public class DataMigration : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("ContentAlternatePart",
                part=>part.Attachable()
                );
            return 1;
        }
    }
}