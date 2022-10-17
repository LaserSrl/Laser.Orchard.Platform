using Laser.Orchard.NwazetIntegration.Aspects;
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
        private readonly IAddressConfigurationService _addressConfigurationService;

        public CheckoutHelperService(
            IShoppingCart shoppingCart,
            IProductPriceService productPriceService,
            IContentManager contentManager,
            IOrderService orderService,
            IWorkContextAccessor workContextAccessor,
            ICurrencyProvider currencyProvider,
            IEnumerable<IOrderAdditionalInformationProvider> orderAdditionalInformationProviders,
            Lazy<IEnumerable<ICheckoutCondition>> checkoutConditions,
            IAddressConfigurationService addressConfigurationService) {

            _shoppingCart = shoppingCart;
            _productPriceService = productPriceService;
            _contentManager = contentManager;
            _orderService = orderService;
            _workContextAccessor = workContextAccessor;
            _currencyProvider = currencyProvider;
            _orderAdditionalInformationProviders = orderAdditionalInformationProviders;
            _checkoutConditions = checkoutConditions;
            _addressConfigurationService = addressConfigurationService;
        }

        private IEnumerable<ICheckoutCondition> CheckoutConditions {
            get {
                return _checkoutConditions.Value.OrderByDescending(cc => cc.Priority);
            }
        }

        public OrderPart CreateOrder(
            CheckoutViewModel cvm,
            string paymentGuid) {

            // Prepare some preliminary information we'll use for the next steps
            var countryId = cvm.SelectedShippingAddressProvider
                .GetShippingCountryId(cvm);
            var country = _addressConfigurationService
                ?.GetCountry(countryId);
            var countryName = country
                ?.Record?.TerritoryInternalRecord.Name;
            _shoppingCart.Country = countryName;
            var postalCode = cvm.ShippingPostalCode;

            // Prepare the parameters we'll need to configure a new Order:
            // Object describing the economic transaction.
            var charge = new PaymentGatewayCharge("Checkout Controller", paymentGuid);
            // Items/products in the cart
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
                    Title = _contentManager.GetItemMetadata(scp.Product).DisplayText,
                    ProductVersion = scp.Product.ContentItem.Version
                });
            // Shipping address is computed based on the selected shipping address provider: each
            // provider handles that on its own when reinflating its information, if it is the
            // selected provider.
            var shippingAddress = cvm.ShippingAddress;
            // Current user
            var user = _workContextAccessor.GetContext().CurrentUser;
            // Context object for providers extending the order.
            // TODO: This should be extended 
            var orderContext = new ExtendedOrderContext {
                WorkContextAccessor = _workContextAccessor,
                ShoppingCart = _shoppingCart,
                Charge = charge,
                ShippingAddress = shippingAddress,
                BillingAddress = cvm.BillingAddress,
                CheckoutViewModel = cvm
            };

            // Create the Order ContentItem. This goes through its own service chain so it
            // doesn't normally fire the normal sequence of content handlers.
            var order = _orderService.CreateOrder(
                charge: charge,
                items: checkoutItems,
                subTotal: _shoppingCart.Subtotal(),
                total: _shoppingCart.Total(),
                taxes: _shoppingCart.Taxes(),
                shippingOption: _shoppingCart.ShippingOption,
                shippingAddress: shippingAddress,
                billingAddress: cvm.BillingAddress,
                customerEmail: cvm.Email,
                customerPhone: cvm.PhonePrefix + " " + cvm.Phone,
                specialInstructions: cvm.SpecialInstructions,
                status: OrderPart.Pending, //.Cancelled,
                trackingUrl:  null,
                isTestOrder: false,
                userId: user != null ? user.Id : -1,
                amountPaid: 0.0M,
                purchaseOrder: "",
                currencyCode: _currencyProvider.CurrencyCode,
                additionalElements: _orderAdditionalInformationProviders
                    .SelectMany(oaip => oaip.PrepareAdditionalInformation(orderContext)));
            
            // Some ContentParts extend Order functionalities by being attached to it, but
            // are not handled directly by OrderService, and it doesn't invoke the entire
            // stack of ContentManagement handlers. Moreover, it doesn't carry within itself
            // the entirety of the chckout context.
            // Here we'll invoke their own methods so that we can update them and their records
            // properly.
            var orderExtensionParts = order.ContentItem.Parts
                .Where(p => p is IOrderExtensionAspect);
            foreach (var exPart in orderExtensionParts) {
                // We have to actually cast rather than use Orchard's extension methods, because
                // those would end up always fetching the first IOrderExtensionAspect out of all
                // the parts implementing it.
                ((IOrderExtensionAspect)exPart).ExtendCreation(cvm);
            }

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
                addressPart.BillingInvoiceRequest = model.BillingAddressVM.InvoiceRequest;
                addressPart.BillingVATNumber = model.BillingAddressVM.VATNumber;
                addressPart.BillingFiscalCode = model.BillingAddressVM.FiscalCode;

            }
            // To properly handle the order's advanced address configuration we need
            // to call again the providers to store the additional data, because when they 
            // are invoked in Nwazet's IOrderService implementation we can't have access
            // to the new information yet. If we ever overhaul that module, we should 
            // account for this extensibility requirement.
            foreach (var oaip in _orderAdditionalInformationProviders) {
                oaip.StoreAdditionalInformation(order);
            }
            order.LogActivity(OrderPart.Event,
                "Order created",
                _workContextAccessor.GetContext()?.CurrentUser?.UserName
                    ?? (!string.IsNullOrWhiteSpace(cvm.Email) ? cvm.Email : "Anonymous"));
            // Unpublish the order
            // We MUST unpublish the order here. The service from Nwazet creates it
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