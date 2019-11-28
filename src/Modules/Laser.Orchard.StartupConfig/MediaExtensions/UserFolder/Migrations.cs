using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Media.UserFolder {
    [OrchardFeature("Laser.Orchard.StartupConfig.MediaExtensions")]
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("OwnedFolderPart", b => b
                .Attachable(false));
            ContentDefinitionManager.AlterTypeDefinition("User",
                cfg => cfg
                    .WithPart("OwnedFolderPart")
                );
            return 1;
        }
    }
}