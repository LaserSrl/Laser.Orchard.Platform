using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class TerritoryAdministrativeTypePartHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        public TerritoryAdministrativeTypePartHandler(
            IRepository<TerritoryAdministrativeTypePartRecord> repository,
            IContentDefinitionManager contentDefinitionManager) {

            _contentDefinitionManager = contentDefinitionManager;

            Filters.Add(StorageFilter.For(repository));
            // attach TerritoryAdministrativeTypePart to ContentTypes that are territories
            Filters.Add(new ActivatingFilter<TerritoryAdministrativeTypePart>(IsTerritory));
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