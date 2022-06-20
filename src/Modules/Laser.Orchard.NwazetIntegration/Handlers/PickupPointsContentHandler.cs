using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointsContentHandler : ContentHandler {

        public PickupPointsContentHandler(
            IRepository<PickupPointPartRecord> repository) {

            Filters.Add(StorageFilter.For(repository));
        }

        // TODO: PickupPointPart will have to affect the Metadata of the
        // content, changing the routes to edit it so the correct controller
        // is used.
    }
}