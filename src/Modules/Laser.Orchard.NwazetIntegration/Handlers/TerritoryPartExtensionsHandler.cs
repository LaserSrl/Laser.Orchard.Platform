using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class TerritoryPartExtensionsHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        public TerritoryPartExtensionsHandler(
            IRepository<TerritoryAddressTypePartRecord> addressTypeRepository,
            IRepository<TerritoryAdministrativeTypePartRecord> adminTypeRepository,
            IContentDefinitionManager contentDefinitionManager) {

            _contentDefinitionManager = contentDefinitionManager;

            Filters.Add(StorageFilter.For(addressTypeRepository));
            Filters.Add(StorageFilter.For(adminTypeRepository));
            
        }

        protected override void Activating(ActivatingContentContext context) {
            // override Activating rather than using Filters to prevent
            // invoking IsTerritory several times.
            if (IsTerritory(context.ContentType)) {
                // Weld parts to "extend" functionality of Territories
                context.Builder.Weld<TerritoryAdministrativeTypePart>();
                context.Builder.Weld<TerritoryAddressTypePart>();
            }
        }

        private bool IsTerritory(string contentType) {
            // return true if type has OrderPart
            var definition = _contentDefinitionManager.GetTypeDefinition(contentType);
            if (definition == null) {
                return false;
            }
            return definition.Parts.Any(ctpd => ctpd.PartDefinition.Name.Equals("TerritoryPart"));
        }
    }
}