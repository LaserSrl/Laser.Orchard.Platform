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
using Laser.Orchard.Cookies.Services;
using Laser.Orchard.Cookies;

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
            
            if (_GTMProductService.ShoulAddEcommerceTags()) {
                CheckoutStep(filterContext)();
            }
        }

        public Action CheckoutStep(ActionExecutedContext filterContext) {
            // Flag to check if I'm using GA4.
            var useGA4 = _GTMProductService.UseGA4();

            // new checkout controller
            if (filterContext.Controller is CheckoutController) {
                if (filterContext.ActionDescriptor
                    .ActionName.Equals("Index", StringComparison.InvariantCultureIgnoreCase)) {
                    // step 1: insert addresses
                    var result = filterContext.Result as ViewResult;
                    if (result != null) {
                        var model = result.Model as CheckoutViewModel;
                        if (model != null) {
                            // this is probably always true
                            if (useGA4) {
                                return GA4AddShape("begin_checkout", GetProducts(model))(filterContext);
                            } else {
                                return AddShape(new { step = 1 }, GetProducts(model))(filterContext);
                            }
                        }
                    }
                } else if (filterContext.ActionDescriptor
                    .ActionName.Equals("Shipping", StringComparison.InvariantCultureIgnoreCase)) {
                    // step 2: select shipping
                    var result = filterContext.Result as ViewResult;
                    if (result != null) {
                        var model = result.Model as CheckoutViewModel;
                        if (model != null) {
                            // this is probably always true
                            return AddShape(new { step = 2 }, GetProducts(model))(filterContext);
                        }
                    }
                } else if (filterContext.ActionDescriptor
                     .ActionName.Equals("Review", StringComparison.InvariantCultureIgnoreCase)) {
                    // step 3: review
                    var result = filterContext.Result as ViewResult;
                    if (result != null) {
                        var model = result.Model as CheckoutViewModel;
                        if (model != null) {
                            // this is probably always true
                            if (model.SelectedShippingOption != null) {
                                return AddShape(new { step = 3,
                                    option = model.SelectedShippingOption.ToString()
                                }, GetProducts(model))(filterContext);
                            } else {
                                return AddShape(new { step = 3 }, GetProducts(model))(filterContext);
                            }
                        }
                    }
                }
            } else {
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
                                var vm = _GTMProductService.GetViewModel(part);
                                vm.Quantity = quantity;
                                return vm;
                            }) ?? new List<IGAProductVM>();

                        object actionField = null;
                        if (model.ShippingOption != null) {
                            var shippingOption = (ShippingOption)model.ShippingOption;
                            // shipping option selected => step 2
                            actionField = new {
                                step = 2,
                                option = shippingOption.ToString()
                            };
                        } else if (!string.IsNullOrWhiteSpace(model.Country)) {
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

                        if (filterContext.Controller.TempData.ContainsKey("CheckoutViewModel")) {
                            var model = (CheckoutViewModel)filterContext.Controller.TempData["CheckoutViewModel"];
                            if (model != null) {
                                // "new" checkout flow
                                return AddShape(new {
                                    step = 4,
                                    options = selectedPos.GetPosName()
                                })(filterContext);
                            }
                        }
                        // step 5 indeed
                        //TODO: this was only really tested for Braintree
                        return AddShape(new {
                            step = 5,
                            options = selectedPos.GetPosName()
                        })(filterContext);
                    }
                }
            }

            // do nothing if this is not a checkout step
            return delegate() { };
        }

        private Func<ActionExecutedContext, Action> AddShape(
            object actionField, IEnumerable<IGAProductVM> products = null) {

            return ctx => delegate () {
                _workContextAccessor
                    .GetContext(ctx)
                    .Layout.Zones.Head
                    .Add(_shapeFactory.GTMCheckoutStep(
                         GTMProducts: products,
                         ActionFieldJSON: JsonConvert.SerializeObject(actionField)));
            };
        }

        private Func<ActionExecutedContext, Action> GA4AddShape(
            string eventName,
            IEnumerable<IGAProductVM> products = null,
            object additionalParams = null) {

            return ctx => delegate () {
                _workContextAccessor
                    .GetContext(ctx)
                    .Layout.Zones.Head
                    .Add(_shapeFactory.GA4CheckoutStep(
                         GTMProducts: products,
                         EventName: eventName,
                         AdditionalParams: additionalParams));
            };
        }

        private IEnumerable<IGAProductVM> GetProducts(CheckoutViewModel checkoutVM) {
            // Check if I'm using GA4
            var useGA4 = _GTMProductService.UseGA4();

            var shopItems = checkoutVM.GetProductQuantities();
            return shopItems
                ?.Select(sci => {
                    var product = sci.Product;
                    var quantity = sci.Quantity;
                    var part = product.As<GTMProductPart>();
                    _GTMProductService.FillPart(part);
                    var vm = _GTMProductService.GetViewModel(part);
                    vm.Quantity = quantity;
                    return vm;
                }) ?? new List<IGAProductVM>();
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
        }
    }
}