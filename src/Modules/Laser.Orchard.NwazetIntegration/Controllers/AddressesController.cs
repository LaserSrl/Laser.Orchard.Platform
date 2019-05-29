using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Laser.Orchard.PaymentGateway.Models;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Controllers {
    public class AddressesController : Controller {
        private readonly IOrderService _orderService;
        private readonly IPosServiceIntegration _posServiceIntegration;
        private readonly IShoppingCart _shoppingCart;
        private readonly IOrchardServices _orchardServices;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly INwazetCommunicationService _nwazetCommunicationService;
        private readonly IPaymentService _paymentService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ITransactionManager _transactionManager;

        private readonly dynamic _shapeFactory;

        public AddressesController(
            IOrderService orderService,
            IPosServiceIntegration posServiceIntegration,
            IPaymentService paymentService,
            IShoppingCart shoppingCart,
            IOrchardServices orchardServices,
            ICurrencyProvider currencyProvider,
            INwazetCommunicationService nwazetCommunicationService,
            IWorkContextAccessor workContextAccessor,
            IShapeFactory shapeFactory,
            ITransactionManager transactionManager) {

            _orderService = orderService;
            _posServiceIntegration = posServiceIntegration;
            _paymentService = paymentService;
            _shoppingCart = shoppingCart;
            _orchardServices = orchardServices;
            _currencyProvider = currencyProvider;
            _nwazetCommunicationService = nwazetCommunicationService;
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;
            _transactionManager = transactionManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

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

        [Themed, OutputCache(NoStore = true, Duration = 0), Authorize]
        public ActionResult MyAddresses() {
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (user == null) {
                // we should never be here, because the AuthorizeAttribute should
                // take care of anonymous users.
                return new HttpUnauthorizedResult(T("Sign In to visualize your saved addresses.").Text);
            }
            var billingAddresses = _nwazetCommunicationService.GetBillingByUser(user);
            var shippingAddresses = _nwazetCommunicationService.GetShippingByUser(user);

            return View((object)_shapeFactory.VewiModel()
                .BillingAddresses(billingAddresses)
                .ShippingAddresses(shippingAddresses));
        }

        [HttpPost,
            OutputCache(NoStore = true, Duration = 0), Authorize,
            ActionName("Delete")]
        public ActionResult Delete(int id, string returnUrl = "") {
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (user == null) {
                // we should never be here, because the AuthorizeAttribute should
                // take care of anonymous users.
                return new HttpUnauthorizedResult(T("Sign In to manage your saved addresses.").Text);
            }

            _nwazetCommunicationService.DeleteAddress(id, user);

            return this.RedirectLocal(returnUrl, () => RedirectToAction("MyAddresses"));
        }

        [HttpGet, Themed, OutputCache(NoStore = true, Duration = 0), Authorize]
        public ActionResult Create() {
            return View(new AddressEditViewModel());
        }
        [HttpPost, Themed, 
            OutputCache(NoStore = true, Duration = 0), Authorize,
            ActionName("Create")]
        public ActionResult CreatePost() {
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (user == null) {
                // we should never be here, because the AuthorizeAttribute should
                // take care of anonymous users.
                return new HttpUnauthorizedResult(T("Sign In to  manage your saved addresses.").Text);
            }

            var newAddress = new AddressEditViewModel();
            if (!TryUpdateModel(newAddress)) {
                _transactionManager.Cancel();
                return View(newAddress);
            }
            _nwazetCommunicationService.AddAddress(newAddress.AddressRecord, user);
            return RedirectToAction("Edit", new { id = newAddress.AddressRecord.Id });
        }


        [HttpGet, Themed, OutputCache(NoStore = true, Duration = 0), Authorize]
        public ActionResult Edit(int id) {
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (user == null) {
                // we should never be here, because the AuthorizeAttribute should
                // take care of anonymous users.
                return new HttpUnauthorizedResult(T("Sign In to manage your saved addresses.").Text);
            }
            var address = _nwazetCommunicationService.GetAddress(id, user);
            if (address == null) {
                return HttpNotFound();
            }
            return View(new AddressEditViewModel(address));
        }

        [HttpPost, Themed,
            OutputCache(NoStore = true, Duration = 0), Authorize,
            ActionName("Edit")]
        public ActionResult EditPost(int id) {
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (user == null) {
                // we should never be here, because the AuthorizeAttribute should
                // take care of anonymous users.
                return new HttpUnauthorizedResult(T("Sign In to  manage your saved addresses.").Text);
            }
            var address = _nwazetCommunicationService.GetAddress(id, user);
            if (address == null) {
                return HttpNotFound();
            }

            var newAddress = new AddressEditViewModel(id);
            if (!TryUpdateModel(newAddress)) {
                _transactionManager.Cancel();
                return View(newAddress);
            }
            _nwazetCommunicationService.AddAddress(newAddress.AddressRecord, user);
            return View(newAddress);
        }
    }
}