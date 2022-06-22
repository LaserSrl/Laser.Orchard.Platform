using Laser.Orchard.NwazetIntegration.Controllers;
using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointsContentHandler : ContentHandler {

        public PickupPointsContentHandler(
            IRepository<PickupPointPartRecord> repository) {

            Filters.Add(StorageFilter.For(repository));
        }
        /*
        // PickupPointPart will have to affect the Metadata of the content,
        // changing the routes to edit it so the correct controller is used.
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var pickupPoint = context.ContentItem.As<PickupPointPart>();
            if (pickupPoint == null) {
                return;
            }

            context.Metadata.CreateRouteValues = new RouteValueDictionary {
                {"Area", PickupPointsAdminController.AreaName},
                {"Controller", PickupPointsAdminController.ControllerName},
                {"Action", PickupPointsAdminController.CreateActionName},
            };
            context.Metadata.EditorRouteValues = new RouteValueDictionary {
                {"Area", PickupPointsAdminController.AreaName},
                {"Controller", PickupPointsAdminController.ControllerName},
                {"Action", PickupPointsAdminController.EditActionName},
                {"Id", pickupPoint.Id},
            };
        }
        */
    }
}