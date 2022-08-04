using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointsContentHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public PickupPointsContentHandler(
            IContentDefinitionManager contentDefinitionManager,
            IRepository<PickupPointPartRecord> pickupPointPartRepository,
            IRepository<PickupPointOrderPartRecord> pickupPointOrderPartRepository) {

            _contentDefinitionManager = contentDefinitionManager;

            Filters.Add(StorageFilter.For(pickupPointPartRepository));
            Filters.Add(StorageFilter.For(pickupPointOrderPartRepository));

            // attach PickupPointOrderPart to ContentTypes that are orders
            Filters.Add(new ActivatingFilter<PickupPointOrderPart>(IsOrder));
        }

        private bool IsOrder(string contentType) {
            // return true if type has OrderPart
            var definition = _contentDefinitionManager.GetTypeDefinition(contentType);
            if (definition == null) {
                return false;
            }
            return definition.Parts.Any(ctpd => ctpd
                .PartDefinition.Name.Equals("OrderPart"));
        }
    }
}