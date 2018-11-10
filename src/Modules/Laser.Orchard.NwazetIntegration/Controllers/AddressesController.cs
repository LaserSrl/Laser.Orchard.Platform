using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.Themes;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Laser.Orchard.NwazetIntegration.Models;
using Orchard;
using Laser.Orchard.PaymentGateway.Models;

namespace Laser.Orchard.NwazetIntegration.Controllers {
    public class AddressesController : Controller {
        private readonly IOrderService _orderService;
        private readonly IPosServiceIntegration _posServiceIntegration;
        private readonly IShoppingCart _shoppingCart;
        private readonly IOrchardServices _orchardServices;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly INwazetCommunicationService _nwazetCommunicationService;
        private readonly IPaymentService _paymentService;
        public AddressesController(
            IOrderService orderService
            , IPosServiceIntegration posServiceIntegration
            , IPaymentService paymentService
            , IShoppingCart shoppingCart
            , IOrchardServices orchardServices
            , ICurrencyProvider currencyProvider
            , INwazetCommunicationService nwazetCommunicationService) {
            _orderService = orderService;
            _posServiceIntegration = posServiceIntegration;
            _paymentService = paymentService;
            _shoppingCart = shoppingCart;
            _orchardServices = orchardServices;
            _currencyProvider = currencyProvider;
            _nwazetCommunicationService = nwazetCommunicationService;
        }
        [Themed]
        public ActionResult Index(AddressesVM model) {
            ActionResult result = null;

            switch (model.Submit) {
                case "cart":
                    result = RedirectToAction("Index", "ShoppingCart", new { area = "Nwazet.Commerce" });
                    break;
                case "save":
                    // costruisce la lista di CheckoutItems in base al contenuto del carrello
                    List<CheckoutItem> items = new List<CheckoutItem>();
                    foreach (var prod in _shoppingCart.GetProducts()) {
                        items.Add(new CheckoutItem {
                            Attributes = prod.AttributeIdsToValues,
                            LinePriceAdjustment = prod.LinePriceAdjustment,
                            OriginalPrice = prod.OriginalPrice,
                            Price = prod.Price,
                            ProductId = prod.Product.Id,
                            PromotionId = prod.Promotion == null ? null : (int?)(prod.Promotion.Id),
                            Quantity = prod.Quantity,
                            Title = prod.Product.ContentItem.As<TitlePart>().Title
                        });
                    }
                    var paymentGuid = Guid.NewGuid().ToString();
                    var charge = new PaymentGatewayCharge("Payment Gateway", paymentGuid);
                    // get Orchard user id
                    var userId = -1;
                    var currentUser = _orchardServices.WorkContext.CurrentUser;
                    if (currentUser != null) {
                        userId = currentUser.Id;
                    }

                    var currency = _currencyProvider.CurrencyCode;
                    var order = _orderService.CreateOrder(
                        charge,
                        items,
                        _shoppingCart.Subtotal(),
                        _shoppingCart.Total(),
                        _shoppingCart.Taxes(),
                        _shoppingCart.ShippingOption,
                        model.ShippingAddress,
                        model.BillingAddress,
                        model.Email,
                        model.PhonePrefix + " " + model.Phone,
                        model.SpecialInstructions,
                        OrderPart.Cancelled,
                        null,
                        false,
                        userId,
                        0,
                        "",
                        currency);
                    order.LogActivity(OrderPart.Event, "Order created");
                    var reason = string.Format("Purchase Order {0}", _posServiceIntegration.GetOrderNumber(order.Id));
                    var payment = new PaymentRecord {
                        Reason = reason,
                        Amount = order.Total,
                        Currency = order.CurrencyCode,
                        ContentItemId = order.Id
                    };
                    var nonce = _paymentService.CreatePaymentNonce(payment);
                    result = RedirectToAction("Pay", "Payment", new { area = "Laser.Orchard.PaymentGateway", nonce = nonce, newPaymentGuid = paymentGuid });
                    break;
                default:
                    model.ShippingAddress = new Address();
                    model.BillingAddress = new Address();
                    var thecurrentUser = _orchardServices.WorkContext.CurrentUser;
                    if (thecurrentUser != null) {
                        model.ListAvailableBillingAddress = _nwazetCommunicationService.GetBillingByUser(thecurrentUser);
                        model.ListAvailableShippingAddress = _nwazetCommunicationService.GetShippingByUser(thecurrentUser);
                        model.Email = thecurrentUser.Email;
                        var cel = _nwazetCommunicationService.GetPhone(thecurrentUser);
                        if (cel.Length == 2) {
                            model.PhonePrefix = cel[0];
                            model.Phone = cel[1];
                        }


                    }
                    result = View("Index", model);
                    break;
            }
            return result;
        }
    }
}