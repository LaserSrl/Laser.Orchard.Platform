using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class OrderPartHandler : ContentHandler {
        public OrderPartHandler() {
            
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