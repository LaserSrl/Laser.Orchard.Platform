using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;
using Laser.Orchard.AdminToolbarExtensions.Models;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.AdminToolbarExtensions {
    [OrchardFeature("Laser.Orchard.AdminToolbarExtensions")]
    public class SummaryAdminToolbarMigrations : DataMigrationImpl {

        public int Create() {

            ContentDefinitionManager.AlterPartDefinition(typeof(SummaryAdminToolbarPart).Name, cfg => cfg
                .Attachable());

            return 1;
        }
    }
}