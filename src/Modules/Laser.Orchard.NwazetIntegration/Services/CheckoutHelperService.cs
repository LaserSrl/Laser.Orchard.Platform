using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class CheckoutHelperService : ICheckoutHelperService {
        private readonly IShoppingCart _shoppingCart;
        private readonly IProductPriceService _productPriceService;
        private readonly IContentManager _contentManager;
        private readonly IOrderService _orderService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly IEnumerable<IOrderAdditionalInformationProvider> _orderAdditionalInformationProviders;
        private readonly Lazy<IEnumerable<ICheckoutCondition>> _checkoutConditions;

        public CheckoutHelperService(
            IShoppingCart shoppingCart,
            IProductPriceService productPriceService,
            IContentManager contentManager,
            IOrderService orderService,
            IWorkContextAccessor workContextAccessor,
            ICurrencyProvider currencyProvider,
            IEnumerable<IOrderAdditionalInformationProvider> orderAdditionalInformationProviders,
            Lazy<IEnumerable<ICheckoutCondition>> checkoutConditions) {

            _shoppingCart = shoppingCart;
            _productPriceService = productPriceService;
            _contentManager = contentManager;
            _orderService = orderService;
            _workContextAccessor = workContextAccessor;
            _currencyProvider = currencyProvider;
            _orderAdditionalInformationProviders = orderAdditionalInformationProviders;
            _checkoutConditions = checkoutConditions;
        }

        private IEnumerable<ICheckoutCondition> CheckoutConditions {
            get {
                return _checkoutConditions.Value.OrderByDescending(cc => cc.Priority);
            }
        }

        public OrderPart CreateOrder(
            AddressesVM model,
            string paymentGuid,
            string countryName = null,
            string postalCode = null) {

            var checkoutItems = _shoppingCart.GetProducts()
                .Select(scp => new CheckoutItem {
                    Attributes = scp.AttributeIdsToValues,
                    LinePriceAdjustment = scp.LinePriceAdjustment,
                    OriginalPrice = scp.OriginalPrice,
                    Price = scp.Product.DiscountPrice >= 0 && scp.Product.DiscountPrice < scp.Product.Price
                        ? _productPriceService.GetDiscountPrice(scp.Product, countryName, postalCode)
                        : _productPriceService.GetPrice(scp.Product, countryName, postalCode),
                    ProductId = scp.Product.Id,
                    PromotionId = scp.Promotion == null ? null : (int?)(scp.Promotion.Id),
                    Quantity = scp.Quantity,
                    Title = _contentManager.GetItemMetadata(scp.Product).DisplayText
                });

            var charge = new PaymentGatewayCharge("Checkout Controller", paymentGuid);
            // 2. Create the Order ContentItem
            var user = _workContextAccessor.GetContext().CurrentUser;
            var orderContext = new OrderContext {
                WorkContextAccessor = _workContextAccessor,
                ShoppingCart = _shoppingCart,
                Charge = charge,
                ShippingAddress = model.ShippingAddress,
                BillingAddress = model.BillingAddress
            };
            var order = _orderService.CreateOrder(
                charge,
                checkoutItems,
                _shoppingCart.Subtotal(),
                _shoppingCart.Total(),
                _shoppingCart.Taxes(),
                _shoppingCart.ShippingOption,
                model.ShippingAddress,
                model.BillingAddress,
                model.Email,
                model.PhonePrefix + " " + model.Phone,
                model.SpecialInstructions,
                OrderPart.Pending, //.Cancelled,
                null,
                false,
                user != null ? user.Id : -1,
                0,
                "",
                _currencyProvider.CurrencyCode,
                _orderAdditionalInformationProviders
                    .SelectMany(oaip => oaip.PrepareAdditionalInformation(orderContext)));

            // 2.1. Verify address information in the AddressOrderPart
            //   (we have to do this explicitly because the management of Order
            //   ContentItems does not go through drivers and such)
            var addressPart = order.As<AddressOrderPart>();
            if (addressPart != null) {
                // shipping info
                if (model.ShippingAddressVM != null) {
                    // may not have a shipping address is shipping isn't required
                    addressPart.ShippingCountryName = model.ShippingAddressVM.Country;
                    addressPart.ShippingCountryId = model.ShippingAddressVM.CountryId;
                    addressPart.ShippingCityName = model.ShippingAddressVM.City;
                    addressPart.ShippingCityId = model.ShippingAddressVM.CityId;
                    addressPart.ShippingProvinceName = model.ShippingAddressVM.Province;
                    addressPart.ShippingProvinceId = model.ShippingAddressVM.ProvinceId;
                    // added information to manage saving in bo
                    addressPart.ShippingAddressIsOptional = false;
                }
                else {
                    addressPart.ShippingAddressIsOptional = true;
                }
                // billing
                addressPart.BillingCountryName = model.BillingAddressVM.Country;
                addressPart.BillingCountryId = model.BillingAddressVM.CountryId;
                addressPart.BillingCityName = model.BillingAddressVM.City;
                addressPart.BillingCityId = model.BillingAddressVM.CityId;
                addressPart.BillingProvinceName = model.BillingAddressVM.Province;
                addressPart.BillingProvinceId = model.BillingAddressVM.ProvinceId;
            }
            // To properly handle the order's advanced address configuration we need
            // to call again the providers to store the additional data, because when they 
            // are invoked in Nwazet's IOrderService implementation we can't have access
            // to the new information yet. If we ever overhaul that module, we should 
            // account for this extensibility requirement.
            foreach (var oaip in _orderAdditionalInformationProviders) {
                oaip.StoreAdditionalInformation(order);
            }
            order.LogActivity(OrderPart.Event, "Order created", _workContextAccessor.GetContext()?.CurrentUser?.UserName ?? (!string.IsNullOrWhiteSpace(model.Email) ? model.Email : "Anonymous"));
            // 2.2. Unpublish the order
            // we unpublish the order here. The service from Nwazet creates it
            // and publishes it. This would cause issues whenever a user leaves
            // mid checkout rather than completing the entire process, because we
            // would end up having unprocessed orders that are created and published.
            // By unpublishing, we practically turn the order in a draft. Later,
            // after processing payments, we publish the order again so it shows
            // in the "normal" queries and lists.
            // Note that this is a workaround for order management that only really
            // works as long as payments are processed and the order published there.
            // In cases where we may not wish to have payments happen when a new order
            // is created, this system should be reworked properly.
            _contentManager.Unpublish(order.ContentItem);

            return order;
        }

        public bool UserMayCheckout(IUser user, out ActionResult redirect) {
            foreach (var condition in CheckoutConditions) {
                // as soon as a condition tells us the user may not checkout, 
                // stop checking
                if (!condition.UserMayCheckout(user, out redirect)) {
                    return false;
                }
            }
            // all conditions returned true
            redirect = null;
            return true;
        }

        public bool UserMayCheckout(IUser user) {
            // there is no condition returning false
            return !CheckoutConditions.Any(cc => !cc.UserMayCheckout(user));
        }
    }
}