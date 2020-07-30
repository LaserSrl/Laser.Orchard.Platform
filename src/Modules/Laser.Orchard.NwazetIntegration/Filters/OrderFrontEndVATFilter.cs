using Nwazet.Commerce.Controllers;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Mvc;
using Orchard.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Filters {
    public class OrderFrontEndVATFilter : FilterProvider, IActionFilter {
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IOrderFrontEndAdditionalInformationProvider> _orderAdditionalInformationProviders;

        public OrderFrontEndVATFilter(
            IContentManager contentManager,
            IEnumerable<IOrderFrontEndAdditionalInformationProvider> orderAdditionalInformationProviders) {

            _contentManager = contentManager;
            _orderAdditionalInformationProviders = orderAdditionalInformationProviders;
        }
        public void OnActionExecuted(ActionExecutedContext filterContext) {
            // This is a horrible hack that we are doing to add further information
            // for frontend view of orders.
            if (filterContext.Controller is OrderSslController
                && filterContext.ActionDescriptor.ActionName.Equals("Show")) {
                var result = filterContext.Result as ShapeResult;
                // if everything is fine, add the front-end VAT information
                if (result != null && result.TempData.ContainsKey("OrderId")) {
                    // this is the same check that is performed in the controller to
                    // decide whether to actually show the order.
                    var part = _contentManager.Get<OrderPart>((int)((dynamic)result.Model).OrderId);
                    ((dynamic)(result).Model)
                        // we also add the OrderPart because for some reason the controller does not
                        .OrderPart(part)
                        // Add all the bits and bobs from the providers
                        .AdditionalMetadataShapes(_orderAdditionalInformationProviders
                            .SelectMany(oaip => oaip.GetAdditionalOrderMetadataShapes(part)))
                        .AdditionalStatusShapes(_orderAdditionalInformationProviders
                            .SelectMany(oaip => oaip.GetAdditionalOrderStatusShapes(part)))
                        .AdditionalAddressesShapes(_orderAdditionalInformationProviders
                            .SelectMany(oaip => oaip.GetAdditionalOrderAddressesShapes(part)))
                        .AdditionalProductShapes(_orderAdditionalInformationProviders
                            .SelectMany(oaip => oaip.GetAdditionalOrderProductsShapes(part)))
                        .AdditionalProductInformation(_orderAdditionalInformationProviders
                            .SelectMany(oaip => oaip.GetAdditionalOrderProductsInformation(part)))
                        .AdditionalOrderTrackingShapes(_orderAdditionalInformationProviders
                            .SelectMany(oaip => oaip.GetAdditionalOrderTrackingShapes(part)));
                }
            }
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
        }
    }
}