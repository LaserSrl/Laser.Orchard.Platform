using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Drivers {
    [OrchardFeature("Laser.Orchard.StartupConfig.RelationshipsEnhancer")]
    public class ContentPickerConnectorDriver : ContentPartDriver<ContentPickerConnectorPart> {

        public ContentPickerConnectorDriver() {

        }
        protected override DriverResult Display(ContentPickerConnectorPart part, string displayType, dynamic shapeHelper) {

            return ContentShape("Parts_ContentPickerConnector", () => shapeHelper.Parts_ContentPickerConnector(ContentPickerConnector: part));

        }
    }
}