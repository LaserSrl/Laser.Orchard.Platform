using Laser.Orchard.NwazetIntegration.Models;
using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class OrderPartHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        public OrderPartHandler(
            IRepository<AddressOrderPartRecord> repository,
            IContentDefinitionManager contentDefinitionManager) {

            _contentDefinitionManager = contentDefinitionManager;

            Filters.Add(StorageFilter.For(repository));
            // attach AddressOrderPart to ContentTypes that are orders
            Filters.Add(new ActivatingFilter<AddressOrderPart>(IsOrder));
        }

        private bool IsOrder(string contentType) {
            // return true if type has OrderPart
            var definition = _contentDefinitionManager.GetTypeDefinition(contentType);
            if (definition == null) {
                return false;
            }
            return definition.Parts.Any(ctpd => ctpd.PartDefinition.Name.Equals("OrderPart"));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var order = context.ContentItem.As<OrderPart>();
            if (order == null) {
                return;
            }

            context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                {"Area", "Nwazet.Commerce" },
                {"Controller", "OrderSsl"},
                {"Action", "Show"},
                {"Id", order.Id}
            };
        }
    }
}