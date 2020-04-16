using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Mvc.Filters;
using System.Web.Mvc;
using Nwazet.Commerce.Controllers;
using Nwazet.Commerce.Models;
using Laser.Orchard.NwazetIntegration.Controllers;
using Laser.Orchard.PaymentGateway.Controllers;
using Orchard.DisplayManagement;
using Orchard;
using Orchard.Mvc;
using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Laser.Orchard.NwazetIntegration.Services;
using Newtonsoft.Json;
using Laser.Orchard.PaymentGateway.ViewModels;

namespace Laser.Orchard.NwazetIntegration.Filters {
    public class CheckoutStepsFilter : FilterProvider, IActionFilter {
        private readonly IEnumerable<IPosService> _posServices;
        private readonly dynamic _shapeFactory;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IGTMProductService _GTMProductService;

        public CheckoutStepsFilter(
            IEnumerable<IPosService> posServices,
            IShapeFactory shapeFactory,
            IWorkContextAccessor workContextAccessor,
            IGTMProductService GTMProductService) {

            _posServices = posServices;
            _shapeFactory = shapeFactory;
            _workContextAccessor = workContextAccessor;
            _GTMProductService = GTMProductService;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
            CheckoutStep(filterContext)();
        }

        public Action CheckoutStep(ActionExecutedContext filterContext) {
            // How to turn this into something flexible:
            // using an ordered collection of providers, each will have a method 
            // that takes the filterContext and returns the "handler" method to perform
            // if there is a match. The first one to return something to execute
            // would win and prevent the others from even checking.
            if (filterContext.Controller is ShoppingCartController
                && filterContext.ActionDescriptor
                    .ActionName.Equals("Index", StringComparison.InvariantCultureIgnoreCase)) {
                // step 1: "Choose shipping option" 
                // or 
                // step 2: "Summary with shipping costs"
                var result = filterContext.Result as ShapeResult;
                if (result != null) {
                    var model = (dynamic)result.Model;
                    // get products in the cart
                    var shopItems = (IEnumerable<dynamic>)model.ShopItems;
                    var products = shopItems
                        ?.Select(sci => {
                            var product = (ProductPart)sci.Product;
                            var quantity = (int)sci.Quantity;
                            var part = product.As<GTMProductPart>();
                            _GTMProductService.FillPart(part);
                            var vm = new GTMProductVM(part);
                            vm.Quantity = quantity;
                            return vm;
                        }) ?? new List<GTMProductVM>();

                    object actionField = null;
                    if (model.ShippingOption != null) {
                        var shippingOption = (ShippingOption)model.ShippingOption;
                        // shipping option selected => step 2
                        actionField = new {
                            step = 2,
                            option = shippingOption.ToString()
                        };
                    } else if (!string.IsNullOrWhiteSpace(model.Country)){
                        // Should select shipping options next => step 1
                        actionField = new { step = 1 };
                    }
                    if (actionField != null) {
                        return AddShape(actionField, products)(filterContext);
                    }
                }
            } else if (filterContext.Controller is AddressesController
                && filterContext.ActionDescriptor
                    .ActionName.Equals("Index", StringComparison.InvariantCultureIgnoreCase)) {
                // step 3: "Add/choose addresses"
                var result = filterContext.Result as ViewResult;
                if (result != null) {
                    var model = result.Model as AddressesVM;
                    if (model != null) {
                        // this is probably always true
                        return AddShape(new { step = 3 })(filterContext);
                    }
                }
            } else if (filterContext.Controller is PaymentController
                && filterContext.ActionDescriptor
                    .ActionName.Equals("Pay", StringComparison.InvariantCultureIgnoreCase)) {
                // step 4: "Summary with addresses"
                var result = filterContext.Result as ViewResult;
                if (result != null) {
                    var model = result.Model as PaymentVM;
                    if (model != null) {
                        // this is probably always true
                        return AddShape(new { step = 4 })(filterContext);
                    }
                }
            } else {
                // possibly step 5: "Payment"
                var selectedPos = _posServices
                    .FirstOrDefault(ps => ps.GetPosActionControllerType() == filterContext.Controller.GetType()
                        && filterContext.ActionDescriptor
                            .ActionName.Equals(ps.GetPosActionName(), StringComparison.InvariantCultureIgnoreCase));
                if (selectedPos != null) {
                    // step 5 indeed
                    //TODO: this was only really tested for Braintree
                    return AddShape(new {
                        step = 5,
                        options = selectedPos.GetPosName() })(filterContext);
                }
            }

            // do nothing if this is not a checkout step
            return delegate() { };
        }

        private Func<ActionExecutedContext, Action> AddShape(
            object actionField, IEnumerable<GTMProductVM> products = null) {

            return ctx => delegate () {
                _workContextAccessor
                    .GetContext(ctx)
                    .Layout.Zones.Head
                    .Add(_shapeFactory.GTMCheckoutStep(
                         GTMProducts: products,
                         ActionFieldJSON: JsonConvert.SerializeObject(actionField)));
            };
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
        }
    }
}