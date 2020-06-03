using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Controllers {
    [Themed] // actions are for the frontend, so themes should be applied
    public class CheckoutController : Controller {
        /*
         This controller provides the actions that map to the ecommerce checkout steps.
         These steps are each in its own action.
          1. Input shipping/billing address (or select from saved ones). This is always
            mandatory.
          2. Select delivery options. This will not be required if nothing that is being
            bought actually requires shipping. Perhaps there are other cases where this
          3. Order review.
          4. Payment.

          There should be a site setting telling whether authentication is required for 
          all orders, overriding the specific flag on each Product that is used in Nwazet.

          There should be a setting telling whether anything has to be shipped. Currently,
          Products may be marked as "Digital", meaning they won't require shipping. The thing
          is, if nothing has to be shipped, the user should only input the billing address, 
          and the resulting order should be marked properly so it doesn't cause faulty 
          interactions with other components of the system, such as shipping integrations.
          The fact that the order requires shipping should be verified at the beginning of
          the checkout process, because it will affect the required steps.
         */
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICheckoutSettingsService _checkoutSettingsService;
        private readonly IAddressConfigurationService _addressConfigurationService;
        private readonly INwazetCommunicationService _nwazetCommunicationService;
        private readonly IEnumerable<IValidationProvider> _validationProviders;
        private readonly IEnumerable<IShippingMethodProvider> _shippingMethodProviders;
        private readonly IShoppingCart _shoppingCart;
        private readonly ICurrencyProvider _currencyProvider;

        public CheckoutController(
            IWorkContextAccessor workContextAccessor,
            ICheckoutSettingsService checkoutSettingsService,
            IAddressConfigurationService addressConfigurationService,
            INwazetCommunicationService nwazetCommunicationService,
            IEnumerable<IValidationProvider> validationProviders,
            IEnumerable<IShippingMethodProvider> shippingMethodProviders,
            IShoppingCart shoppingCart,
            ICurrencyProvider currencyProvider) {

            _workContextAccessor = workContextAccessor;
            _checkoutSettingsService = checkoutSettingsService;
            _addressConfigurationService = addressConfigurationService;
            _nwazetCommunicationService = nwazetCommunicationService;
            _validationProviders = validationProviders;
            _shippingMethodProviders = shippingMethodProviders;
            _shoppingCart = shoppingCart;
            _currencyProvider = currencyProvider;
        }

        public ActionResult CheckoutStart() {
            return RedirectToAction("Index");
        }

        public ActionResult Index(CheckoutViewModel model) {
            // This will be the entry point for the checkout process.
            // This method should probably have parameters to handle displaying its form
            // with some information already in it in case of validation errors when posting
            // it.
            // Try to fetch the model from TempData to handle the case where we have been
            // redirected here.
            if (TempData.ContainsKey("CheckoutViewModel")) {
                model = (CheckoutViewModel)TempData["CheckoutViewModel"];
            }
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (!_checkoutSettingsService.UserMayCheckout(user)) {
                // TODO: change the UserMayCheckout
                // Have a method that returns the action I should redirect the user to
                // in case they can't checkout (e.g. LogOn or AccessDenied)
                // redirect to login, perhaps with a message
            }
            if (user != null) {
                // If the user is authenticated, set the model's email and any other information
                // we can get from the user's contact
                model.Email = user.Email;
                var cel = _nwazetCommunicationService.GetPhone(user);
                if (cel.Length == 2) {
                    model.PhonePrefix = cel[0];
                    model.Phone = cel[1];
                }
                // also load the list of existing addresses for them
                model.ListAvailableBillingAddress = 
                    _nwazetCommunicationService.GetBillingByUser(user);
                // we are only going to load the shipping addresses if shipping is required
                if(IsShippingRequired()) {
                    model.ListAvailableShippingAddress = 
                        _nwazetCommunicationService.GetShippingByUser(user);
                }
            }
            model.BillingAddressVM = CreateVM(AddressRecordType.BillingAddress);
            // test whether shipping will be required for the order, because that will change
            // what must be displayed for the addresses as well as what happens when we go ahead
            // with the checkout: if no shipping is required, we can go straight to order review
            // and then payment.
            if (IsShippingRequired() && model.ShippingAddressVM == null) {
                model.ShippingAddressVM = CreateVM(AddressRecordType.ShippingAddress);
            }
            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(CheckoutViewModel model) {
            // Depending on whether shipping is required or not, the validation of what has been
            // input changes, because if there is no shipping there's no need for the shipping
            // address.
            var shippingRequired = IsShippingRequired(); //we'll reuse this
            var validationSuccess = TryUpdateModel(model.BillingAddressVM)
                && ValidateVM(model.BillingAddressVM);
            if (shippingRequired) {
                validationSuccess &= TryUpdateModel(model.ShippingAddressVM)
                    && ValidateVM(model.ShippingAddressVM);
            }
            validationSuccess &= ValidateAddresses(model);
            if (!validationSuccess) {
                // don't move on, but rather leave the user on this form
                return View(model);
            }
            // in case validation is successful, if a user exists, try to store the 
            // addresses they just configured.
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (user != null) {
                // TODO: save addresses
            }
            // In case validation is successful, depending on whether shipping is required, we
            // should redirect to a different action/step. 
            // If shipping is required, we should redirect to an action that lets the user select 
            // their preferred shipping method. If only one method is configured, the user should
            // still be made to go through that step. The selection of the list of available methods
            // can make use of the address the user has selected for shipping.
            // If no shipping is required we can move on to reviewing the order.
            // Put the model we validated in TempData so it can be reused in the next action.
            TempData["CheckoutViewModel"] = model;
            if (IsShippingRequired()) {
                return RedirectToAction("Shipping");
            }
            return RedirectToAction("Review");
        }

        public ActionResult Shipping(CheckoutViewModel model) {
            // In this step the user will select the shipping method from a list
            // Try to fetch the model from TempData to handle the case where we have been
            // redirected here.
            if (TempData.ContainsKey("CheckoutViewModel")) {
                model = (CheckoutViewModel)TempData["CheckoutViewModel"];
            }
            if (model.ShippingAddressVM == null 
                && !string.IsNullOrWhiteSpace(model.SerializedAddresses)) {
                model.DecodeAddresses();
            }
            if (model.ShippingAddressVM != null) {
                var productQuantities = _shoppingCart
                    .GetProducts()
                    .Where(p => p.Quantity > 0)
                    .ToList();
                // Hack: based on the address coming in model.ShippingAddressVM, we can 
                // compute the actual destinations to be used at this stage
                var countryName = _addressConfigurationService
                    ?.GetCountry(model.ShippingAddressVM.CountryId)
                    ?.Record?.TerritoryInternalRecord.Name;
                // get all possible providers for shipping methods
                var shippingMethods = _shippingMethodProviders
                    .SelectMany(p => p.GetShippingMethods())
                    .ToList();
                // Test those providers with nwazet's default interface.
                var allShippingOptions = ShippingService
                    .GetShippingOptions(
                        shippingMethods,
                        productQuantities,
                        countryName,
                        model.ShippingAddressVM.PostalCode,
                        _workContextAccessor).ToList();
                // Those tests done like that cannot be very reliable with respect to
                // territories' configuration, unless we configure a hierarchy of postal
                // codes. Rather than do that, we are going to do an hack on the PostalCode
                // that will let a shipping criterion parse out the territory Ids.
                allShippingOptions
                    .AddRange(ShippingService
                        .GetShippingOptions(
                            shippingMethods,
                            productQuantities,
                            countryName,
                            $"{model.ShippingAddressVM.PostalCode};" +
                                $"{model.ShippingAddressVM.CountryId};" +
                                $"{model.ShippingAddressVM.ProvinceId};" +
                                $"{model.ShippingAddressVM.CityId}",
                            _workContextAccessor));
                // remove duplicate shipping options
                model.AvailableShippingOptions = allShippingOptions.Distinct().ToList();
                // to correctly display prices, the view will need the currency provider
                model.CurrencyProvider = _currencyProvider;
                return View(model);
            }
            // to get here something must have gone very wrong. Perhaps the user
            // is trying to select a shipping method where no shipping is required.
            // Put the model we validated in TempData so it can be reused in the next action.
            TempData["CheckoutViewModel"] = model;
            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Shipping")]
        public ActionResult ShippingPOST(CheckoutViewModel model) {
            // validate the choice of shipping method then redirect to the action that lets
            // the user review their order.
            return View(model);
        }

        public ActionResult Review() {
            // In this step the user will be able to review their order, and finally go ahead
            // and finalize. We may want to have a null payment provider for free stuff?

            return null;
        }

        [HttpPost, ActionName("Review")]
        public ActionResult ReviewPOST() {
            // redirect the user to their payment method of choice
            // is there any validation that should be happening here?
            return null;
        }


        private bool IsShippingRequired() {
            //TODO
            // This should get the current order under process, as well as any other relevant
            // setting, as well as the user object, to determine whether anything has to be 
            // shipped.
            return true;
        }
        private AddressEditViewModel CreateVM() {
            return AddressEditViewModel.CreateVM(_addressConfigurationService);
        }
        private AddressEditViewModel CreateVM(AddressRecordType addressRecordType) {
            return AddressEditViewModel.CreateVM(_addressConfigurationService, addressRecordType);
        }

        private bool ValidateVM(AddressEditViewModel vm) {
            bool response = true;
            foreach (var valP in _validationProviders) {
                if (!valP.Validate(vm)) {
                    response = false;
                }
            }
            return response;
        }
        private bool ValidateAddresses(CheckoutViewModel vm) {
            return ValidateVM(vm.AsAddressesVM());
        }
        private bool ValidateVM(AddressesVM vm) {
            bool response = true;
            foreach (var valP in _validationProviders) {
                var result = valP.Validate(vm);
                if (result.Count() > 0) {
                    response = false;
                }
                foreach (var error in valP.Validate(vm)) {
                    ModelState.AddModelError("_FORM", error.Text);
                }
            }
            return response;
        }

    }
}