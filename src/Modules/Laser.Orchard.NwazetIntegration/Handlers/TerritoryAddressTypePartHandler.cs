using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class TerritoryAddressTypePartHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        public TerritoryAddressTypePartHandler(
            IRepository<TerritoryAddressTypePartRecord> repository,
            IContentDefinitionManager contentDefinitionManager) {

            _contentDefinitionManager = contentDefinitionManager;

            Filters.Add(StorageFilter.For(repository));
            // attach TerritoryAddressTypePart to ContentTypes that are territories
            Filters.Add(new ActivatingFilter<TerritoryAddressTypePart>(IsTerritory));
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