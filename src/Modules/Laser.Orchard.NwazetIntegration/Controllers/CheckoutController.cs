using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Laser.Orchard.PaymentGateway.Models;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Mvc.Routes;
using Orchard.Themes;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly IAddressConfigurationService _addressConfigurationService;
        private readonly INwazetCommunicationService _nwazetCommunicationService;
        private readonly IEnumerable<IValidationProvider> _validationProviders;
        private readonly IEnumerable<IShippingMethodProvider> _shippingMethodProviders;
        private readonly IShoppingCart _shoppingCart;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IPosService> _posServices;
        private readonly ICheckoutHelperService _checkoutHelperService;
        private readonly ShellSettings _shellSettings;
        private readonly IProductPriceService _productPriceService;
        private readonly INotifier _notifier;
        private readonly IEnumerable<ICheckoutExtensionProvider> _checkoutExtensionProviders;

        public CheckoutController(
            IWorkContextAccessor workContextAccessor,
            IAddressConfigurationService addressConfigurationService,
            INwazetCommunicationService nwazetCommunicationService,
            IEnumerable<IValidationProvider> validationProviders,
            IEnumerable<IShippingMethodProvider> shippingMethodProviders,
            IShoppingCart shoppingCart,
            ICurrencyProvider currencyProvider,
            IContentManager contentManager,
            IEnumerable<IPosService> posServices,
            ICheckoutHelperService checkoutHelperService,
            ShellSettings shellSettings,
            IProductPriceService productPriceService,
            INotifier notifier,
            IEnumerable<ICheckoutExtensionProvider> checkoutExtensionProviders) {

            _workContextAccessor = workContextAccessor;
            _addressConfigurationService = addressConfigurationService;
            _nwazetCommunicationService = nwazetCommunicationService;
            _validationProviders = validationProviders;
            _shippingMethodProviders = shippingMethodProviders;
            _shoppingCart = shoppingCart;
            _currencyProvider = currencyProvider;
            _contentManager = contentManager;
            _posServices = posServices;
            _checkoutHelperService = checkoutHelperService;
            _shellSettings = shellSettings;
            _productPriceService = productPriceService;
            _notifier = notifier;
            _checkoutExtensionProviders = checkoutExtensionProviders;

            if (!string.IsNullOrEmpty(_shellSettings.RequestUrlPrefix))
                _urlPrefix = new UrlPrefix(_shellSettings.RequestUrlPrefix);

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private readonly UrlPrefix _urlPrefix;
        private string RedirectUrl {
            get {
                var request = _workContextAccessor.GetContext().HttpContext.Request;
                if (request.UrlReferrer != null) {
                    if (request.UrlReferrer.Host.Equals(request.Url.Host, StringComparison.OrdinalIgnoreCase)) {
                        return request.UrlReferrer.ToString();
                    }
                }
                if (_urlPrefix != null) {
                    return _urlPrefix.PrependLeadingSegments("~");
                }
                return "~/";
            }
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult CheckoutStart(FormCollection formCollection) {
            var extensionContext = new CheckoutExtensionContext() {
                ValueProvider = ValueProvider,
                ModelState = ModelState,
                FormCollection = formCollection
            };
            foreach (var provider in _checkoutExtensionProviders) {
                provider.ProcessAdditionalCheckoutStartInformation(extensionContext);
            }
            if (!ModelState.IsValid) {
                TempData["ModelState"] = ModelState;
                // redirect back to the page the user is coming from
                // or the cart if it's not from within the tenant
                var referrer = Request.UrlReferrer;
                if (Request.Url.Host == referrer.Host) {
                    return Redirect(referrer.ToString());
                }
                return RedirectToAction("Index", "ShoppingCart", new { area = "Nwazet.Commerce" });
            }

            var user = _workContextAccessor.GetContext().CurrentUser;
            if (!_checkoutHelperService.UserMayCheckout(user, out ActionResult redirect)) {
                if (redirect != null) {
                    return redirect;
                }
                return Redirect(RedirectUrl);
            }
            TempData["CheckoutViewModel"] = new CheckoutViewModel() {
                UseDefaultAddress = true,
                UseDefaultShipping = true
            };
            return RedirectToAction("Index");
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index(CheckoutViewModel model) {
            // This will be the entry point for the checkout process.
            // This method should probably have parameters to handle displaying its form
            // with some information already in it in case of validation errors when posting
            // it.
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (!_checkoutHelperService.UserMayCheckout(user, out ActionResult redirect)) {
                if (redirect != null) {
                    return redirect;
                }
                return Redirect(RedirectUrl);
            }
            // Try to fetch the model from TempData to handle the case where we have been
            // redirected here.
            if (TempData.ContainsKey("CheckoutViewModel")) {
                model = (CheckoutViewModel)TempData["CheckoutViewModel"];
            }
            model.ShippingRequired = IsShippingRequired();
            if (user != null) {
                // If the user is authenticated, set the model's email and any other information
                // we can get from the user's contact, but avoid overwriting previous input
                if (string.IsNullOrWhiteSpace(model.Email)) {
                    model.Email = user.Email;
                }
                if (string.IsNullOrWhiteSpace(model.Phone)) {
                    var cel = _nwazetCommunicationService.GetPhone(user);
                    if (cel.Length == 2) {
                        model.PhonePrefix = cel[0];
                        model.Phone = cel[1];
                    }
                }
                // also load the list of existing addresses for them
                model.ListAvailableBillingAddress =
                    _nwazetCommunicationService.GetBillingByUser(user);
                // we are only going to load the shipping addresses if shipping is required
                if (model.ShippingRequired) {
                    model.ListAvailableShippingAddress =
                        _nwazetCommunicationService.GetShippingByUser(user);
                }
            }
            // attempt to shortcircuit the actions
            if (model.UseDefaultAddress) {
                // if the user has saved addresses, preselect the ones used most recently
                if (model.ListAvailableBillingAddress.Any()
                    && (!model.ShippingRequired || (model.ShippingRequired && model.ListAvailableShippingAddress.Any()))) {
                    model.BillingAddressVM = new AddressEditViewModel(model
                        .ListAvailableBillingAddress
                        // pick the one used/updated most recently
                        .OrderByDescending(a => a.TimeStampUTC)
                        .First());
                    if (model.ShippingRequired) {
                        model.ShippingAddressVM = new AddressEditViewModel(model
                            .ListAvailableShippingAddress
                            // pick the one used/updated most recently
                            .OrderByDescending(a => a.TimeStampUTC)
                            .First());
                    }
                    // redirect to next step
                    // Put the model we validated in TempData so it can be reused in the next action.
                    TempData["CheckoutViewModel"] = model;
                    if (model.ShippingRequired) {
                        // Set values into the ShoppingCart storage
                        var country = _addressConfigurationService
                            ?.GetCountry(model.ShippingAddressVM.CountryId);
                        _shoppingCart.Country = _contentManager.GetItemMetadata(country).DisplayText;
                        _shoppingCart.ZipCode = model.ShippingAddressVM.PostalCode;
                    }
                    // we redirect to this post action, where we do validation
                    return IndexPOST(model);
                }

            }

            model.BillingAddressVM = CreateVM(AddressRecordType.BillingAddress, model.BillingAddressVM);
            // test whether shipping will be required for the order, because that will change
            // what must be displayed for the addresses as well as what happens when we go ahead
            // with the checkout: if no shipping is required, we can go straight to order review
            // and then payment.
            if (model.ShippingRequired) {
                model.ShippingAddressVM = CreateVM(AddressRecordType.ShippingAddress, model.ShippingAddressVM);
            }
            InjectServices(model);
            FinalizeVM(model);
            return View(model);
        }

        [HttpPost, ActionName("Index")]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult IndexPOST(CheckoutViewModel model) {
            // Depending on whether shipping is required or not, the validation of what has been
            // input changes, because if there is no shipping there's no need for the shipping
            // address.
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (!_checkoutHelperService.UserMayCheckout(user, out ActionResult redirect)) {
                if (redirect != null) {
                    return redirect;
                }
                return Redirect(RedirectUrl);
            }
            // check if the user is trying to reset the selected addresses to select different
            // ones.
            if (model.ResetAddresses) {
                // reset shipment to redo the calculation correctly, 
                // removing from the total the shipment that will have to be reselected
                _shoppingCart.ShippingOption = null;

                ReinflateViewModelAddresses(model);
                // Put the model we validated in TempData so it can be reused in the next action.
                TempData["CheckoutViewModel"] = model;
                return RedirectToAction("Index");
            }
            model.ShippingRequired = IsShippingRequired(); //we'll reuse this
            // Ensure address types are initialized correctly 
            model.BillingAddressVM.AddressType = AddressRecordType.BillingAddress;
            model.BillingAddressVM.AddressRecord.AddressType = AddressRecordType.BillingAddress;
            if (model.ShippingAddressVM != null) {
                model.ShippingAddressVM.AddressType = AddressRecordType.ShippingAddress;
                model.ShippingAddressVM.AddressRecord.AddressType = AddressRecordType.ShippingAddress;
            }
            // validate
            var validationSuccess = ValidateVM(model);
            if (!validationSuccess) {
                // don't move on, but rather leave the user on this form
                model.BillingAddressVM = CreateVM(AddressRecordType.BillingAddress, model.BillingAddressVM);
                if (model.ShippingRequired) {
                    model.ShippingAddressVM = CreateVM(AddressRecordType.ShippingAddress, model.ShippingAddressVM);
                }
                InjectServices(model);
                if (user != null) {
                    // also load the list of existing addresses for them
                    model.ListAvailableBillingAddress =
                        _nwazetCommunicationService.GetBillingByUser(user);
                    // we are only going to load the shipping addresses if shipping is required
                    if (model.ShippingRequired) {
                        model.ListAvailableShippingAddress =
                            _nwazetCommunicationService.GetShippingByUser(user);
                    }
                }
                return View(model);
            }
            // in case validation is successful, if a user exists, try to store the 
            // addresses they just configured.
            if (user != null) {
                if (model.BillingAddressVM != null && model.BillingAddressVM.AddressRecord != null) {
                    var countryTP = _addressConfigurationService.GetCountry(model.BillingAddressVM.CountryId);
                    model.BillingAddressVM.Country = _contentManager.GetItemMetadata(countryTP).DisplayText;

                    if (model.BillingAddressVMListAddress > 0) {
                        model.BillingAddressVM.AddressRecord.Id = model.BillingAddressVMListAddress;
                    }
                    _nwazetCommunicationService.AddAddress(model.BillingAddressVM.AddressRecord, user);
                }
                if (model.ShippingAddressVM != null && model.ShippingAddressVM.AddressRecord != null) {
                    var countryTP = _addressConfigurationService.GetCountry(model.ShippingAddressVM.CountryId);
                    model.ShippingAddressVM.Country = _contentManager.GetItemMetadata(countryTP).DisplayText;
                    if (model.ShippingAddressVMListAddress > 0) {
                        model.ShippingAddressVM.AddressRecord.Id = model.ShippingAddressVMListAddress;
                    }
                    _nwazetCommunicationService.AddAddress(model.ShippingAddressVM.AddressRecord, user);
                }
                if (!string.IsNullOrWhiteSpace(model.PhonePrefix) || !string.IsNullOrWhiteSpace(model.Phone)) {
                    _nwazetCommunicationService.SetPhone(model.PhonePrefix, model.Phone, user);
                }
            }
            // In case validation is successful, depending on whether shipping is required, we
            // should redirect to a different action/step. 
            // If shipping is required, we should redirect to an action that lets the user select 
            // their preferred shipping method. If only one method is configured, the user should
            // still be made to go through that step. The selection of the list of available methods
            // can make use of the address the user has selected for shipping.
            // If no shipping is required we can move on to reviewing the order.
            // At this stage, since we also have the correct address, we can correctly compute TAX
            // TODO: compute VAT
            // Put the model we validated in TempData so it can be reused in the next action.
            TempData["CheckoutViewModel"] = model;
            if (IsShippingRequired()) {
                // Set values into the ShoppingCart storage
                var country = _addressConfigurationService
                    ?.GetCountry(model.ShippingAddressVM.CountryId);
                _shoppingCart.Country = _contentManager.GetItemMetadata(country).DisplayText;
                _shoppingCart.ZipCode = model.ShippingAddressVM.PostalCode;
                return RedirectToAction("Shipping");
            }
            return RedirectToAction("Review");
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Shipping(CheckoutViewModel model) {
            // In this step the user will select the shipping method from a list
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (!_checkoutHelperService.UserMayCheckout(user, out ActionResult redirect)) {
                if (redirect != null) {
                    return redirect;
                }
                return Redirect(RedirectUrl);
            }
            // Try to fetch the model from TempData to handle the case where we have been
            // redirected here.
            if (TempData.ContainsKey("CheckoutViewModel")) {
                model = (CheckoutViewModel)TempData["CheckoutViewModel"];
            }

            // Check if the mail returned an error: "A shipment has not been selected"
            if (TempData.ContainsKey("ShippingError")) {
                ModelState.AddModelError("_FORM", TempData["ShippingError"].ToString());
            }

            // deserialize addresses
            ReinflateViewModelAddresses(model);

            model.ShippingRequired = IsShippingRequired();
            if (model.ShippingAddressVM != null) {
                var productQuantities = _shoppingCart
                    .GetProducts()
                    .Where(p => p.Quantity > 0)
                    .ToList();
                // Hack: based on the address coming in model.ShippingAddressVM, we can 
                // compute the actual destinations to be used at this stage
                var country = _addressConfigurationService
                    ?.GetCountry(model.ShippingAddressVM.CountryId);
                var countryName = country
                    ?.Record?.TerritoryInternalRecord.Name;
                // we should make sure that the country name is filled in for the shipping
                // address view model, so we can display it.
                if (string.IsNullOrWhiteSpace(model.ShippingAddressVM.Country)) {
                    model.ShippingAddressVM.Country = _contentManager.GetItemMetadata(country).DisplayText;
                }
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
                model.AvailableShippingOptions = allShippingOptions.Distinct(new ShippingOption.ShippingOptionComparer()).ToList();
                // See whether we are trying to short circuit the checkout steps
                if (model.UseDefaultShipping) {
                    // if there is no option selected, and there is only one possible option, we can skip
                    // the selection. If the user is trying to reset their selection, there is a selected
                    // option so it should not trigger this condition
                    if (string.IsNullOrWhiteSpace(model.ShippingOption) && model.AvailableShippingOptions.Count == 1) {
                        model.ShippingOption = model.AvailableShippingOptions.First().FormValue;
                        return ShippingPOST(model);
                    }
                }
                // to correctly display prices, the view will need the currency provider
                model.CurrencyProvider = _currencyProvider;
                // encode addresses so we can hide them in the form
                model.EncodeAddresses();
                // make sure services are injected so they may be used
                InjectServices(model);
                return View(model);
            }
            // to get here something must have gone very wrong. Perhaps the user
            // is trying to select a shipping method where no shipping is required.
            // More likely, a refresh of the shipping page messed things up for us.
            // Either way, go through the index in an attempt to properly repopulate 
            // the addresses
            model.UseDefaultAddress = true;
            // Put the model in TempData so it can be reused in the next action.
            TempData["CheckoutViewModel"] = model;
            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Shipping")]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult ShippingPOST(CheckoutViewModel model) {
            // validate the choice of shipping method then redirect to the action that lets
            // the user review their order.
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (!_checkoutHelperService.UserMayCheckout(user, out ActionResult redirect)) {
                if (redirect != null) {
                    return redirect;
                }
                return Redirect(RedirectUrl);
            }
            // Addresses come from the form as encoded in a single thing, because at
            // this stage the user will have already selected them earlier.
            ReinflateViewModelAddresses(model);
            // check if the user is trying to reset the selected shipping option.
            if (model.ResetShipping || string.IsNullOrWhiteSpace(model.ShippingOption)) {
                // Put the model we validated in TempData so it can be reused in the next action.
                _shoppingCart.ShippingOption = null;
                TempData["CheckoutViewModel"] = model;

                // used tempdata because doing the "redirecttoaction" doesn't keep the modelstate value saved
                // it is an error only if I am not doing a reset shipping, 
                // because if I am doing a reset shipping it is normal for the shipping options to be null
                if (!model.ResetShipping) {
                    TempData["ShippingError"] = T("Select a shipment to proceed with your order").Text;
                }
                return RedirectToAction("Shipping");
            }
            // the selected shipping option
            if (string.IsNullOrWhiteSpace(model.ShippingOption)) {
                // TODO: they selected no shipping!
                // redirect them somewhere
            }
            var selectedOption = ShippingService.RebuildShippingOption(model.ShippingOption);
            _shoppingCart.ShippingOption = selectedOption;
            model.SelectedShippingOption = selectedOption;
            model.ShippingRequired = IsShippingRequired();

            // Put the model in TempData so it can be reused in the next action.
            TempData["CheckoutViewModel"] = model;
            return RedirectToAction("Review");
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Review(CheckoutViewModel model) {
            // In this step the user will be able to review their order, and finally go ahead
            // and finalize. We may want to have a null payment provider for free stuff?
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (!_checkoutHelperService.UserMayCheckout(user, out ActionResult redirect)) {
                if (redirect != null) {
                    return redirect;
                }
                return Redirect(RedirectUrl);
            }
            // Try to fetch the model from TempData to handle the case where we have been
            // redirected here.
            if (TempData.ContainsKey("CheckoutViewModel")) {
                model = (CheckoutViewModel)TempData["CheckoutViewModel"];
            }
            // decode stuff that may be encoded
            ReinflateViewModelAddresses(model);
            model.ShippingRequired = IsShippingRequired();
            if (model.ShippingRequired && model.SelectedShippingOption == null) {
                if (string.IsNullOrWhiteSpace(model.ShippingOption)) {
                    // Here we need a selected shipping method, but we don't have it somehow
                    // so we redirect back to shipping selection
                    // Put the model in TempData so it can be reused in the next action.
                    // This is an attempt to skip shipping, so try to shortcircuit.
                    model.UseDefaultShipping = true;
                    TempData["CheckoutViewModel"] = model;
                    return RedirectToAction("Shipping");
                }
                var selectedOption = ShippingService.RebuildShippingOption(model.ShippingOption);
                _shoppingCart.ShippingOption = selectedOption;
                model.SelectedShippingOption = selectedOption;
            }
            // We will need to display:
            // 1. The summary of all the user's choices up until this point.
            //    (that's already in the model)
            // 2. The list of buttons for the available payment options.
            model.PosServices = _posServices;

            // encode addresses so we can hide them in the form
            model.EncodeAddresses();
            // make sure services are injected so they may be used
            InjectServices(model);
            return View(model);
        }

        [HttpPost, ActionName("Review")]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult ReviewPOST(CheckoutViewModel model) {
            // redirect the user to their payment method of choice
            // is there any validation that should be happening here?
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (!_checkoutHelperService.UserMayCheckout(user, out ActionResult redirect)) {
                if (redirect != null) {
                    return redirect;
                }
                return Redirect(RedirectUrl);
            }
            if (string.IsNullOrWhiteSpace(model.SelectedPosService)) {
                // the user selected no payment method
                _notifier.Error(T("Impossible to start payment with the selected provider. Please try again."));
                TempData["CheckoutViewModel"] = model;
                return RedirectToAction("Review");
            }
            // get the pos by name
            var selectedService = _posServices
                .FirstOrDefault(ps => ps.GetPosName()
                    .Equals(model.SelectedPosService, StringComparison.OrdinalIgnoreCase));
            if (selectedService == null) {
                // data got corrupted?
                _notifier.Error(T("Impossible to start payment with the selected provider. Please try again."));
                TempData["CheckoutViewModel"] = model;
                return RedirectToAction("Review");
            }
            // Re-validate the entire model to be safe
            ReinflateViewModelAddresses(model);
            // later we'll need the country and postal code
            var countryName = !string.IsNullOrWhiteSpace(model.ShippingAddressVM?.Country)
                ? model.ShippingAddressVM?.Country
                : (!string.IsNullOrWhiteSpace(model.BillingAddressVM?.Country)
                    ? model.BillingAddressVM?.Country
                    : "");
            _shoppingCart.Country = countryName;
            var postalCode = model.ShippingAddressVM != null
                ? model.ShippingAddressVM.PostalCode
                : model.BillingAddressVM.PostalCode;
            _shoppingCart.ZipCode = postalCode;
            // Validate ShippingOption
            model.ShippingRequired = IsShippingRequired();
            if (model.ShippingRequired && model.SelectedShippingOption == null) {
                if (string.IsNullOrWhiteSpace(model.ShippingOption)) {
                    // TODO: manage this error condition
                    // Here we need a selected shipping method, but we don't have it somehow
                }

                // TODO: check this: we are reinflating from the model but maybe we have
                // this in _shoppingCart?
                var selectedOption = ShippingService.RebuildShippingOption(model.ShippingOption);
                _shoppingCart.ShippingOption = selectedOption;
                model.SelectedShippingOption = selectedOption;
            }
            // TODO: Validate Cart
            // Here we want to:
            // 1. Create the PayementGatewayCharge we'll use for events
            var paymentGuid = Guid.NewGuid().ToString();
            // 2. Create the Order ContentItem
            var order = _checkoutHelperService.CreateOrder(model.AsAddressesVM(), paymentGuid, countryName, postalCode);

            // 3. Don't attach the address from the Order to the Contact for
            //   the user, because that was done when inputing the address.
            // 3.1. If there is a User, we may wish to add their email and phone
            //   number to the Contact.
            // 4. Create the payment record for the Order.
            var reason = string.Format("Purchase Order {0}", order.OrderKey);
            var payment = new PaymentRecord {
                Reason = reason,
                Amount = order.Total,
                Currency = order.CurrencyCode,
                ContentItemId = order.Id
            };
            // 4.1. Invoke the StartPayment method for the selected IPosService.
            payment = selectedService.StartPayment(payment, paymentGuid);
            // 5. Get form the IPosService the controller URL and redirect there.
            // Put the model in TempData so it can be reused in the next action.
            TempData["CheckoutViewModel"] = model;
            return Redirect(selectedService.GetPosActionUrl(payment.Guid));
        }

        private bool IsShippingRequired() {
            //TODO
            // This should get the current order under process, as well as any other relevant
            // setting, as well as the user object, to determine whether anything has to be 
            // shipped.
            var required = _workContextAccessor.GetContext().CurrentSite
                .As<CheckoutSettingsPart>().ShippingIsRequired;
            if (required) {
                // the site settings short-circuits the tests
                return true;
            }
            // Any phyisical product
            required = _shoppingCart.GetProducts().Any(pq => !pq.Product.IsDigital);
            return required;
        }
        private AddressEditViewModel CreateVM() {
            return AddressEditViewModel.CreateVM(_addressConfigurationService);
        }
        private AddressEditViewModel CreateVM(AddressRecordType addressRecordType) {
            return AddressEditViewModel.CreateVM(_addressConfigurationService, addressRecordType);
        }
        private AddressEditViewModel CreateVM(AddressRecordType addressRecordType, AddressEditViewModel vm) {
            return AddressEditViewModel.CreateVM(_addressConfigurationService, addressRecordType, vm);
        }
        private void ReinflateViewModelAddresses(CheckoutViewModel vm) {
            // addresses
            CheckoutViewModel.ReinflateViewModelAddresses(vm, _contentManager, _addressConfigurationService);
        }

        private void InjectServices(CheckoutViewModel vm) {

            // to correctly display prices, the view will need the currency provider
            vm.CurrencyProvider = _currencyProvider;
            vm.ShoppingCart = _shoppingCart;
            vm.ProductPriceService = _productPriceService;
        }

        private bool ValidateVM(CheckoutViewModel vm) {
            // validate Email
            var validEmail = !string.IsNullOrWhiteSpace(vm.Email)
                && Regex.IsMatch(vm.Email, Constants.EmailPattern,
                    RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var validationSuccess = TryUpdateModel(vm.BillingAddressVM)
                && ValidateVM(vm.BillingAddressVM);
            if (vm.ShippingRequired) {
                validationSuccess &= TryUpdateModel(vm.ShippingAddressVM)
                    && ValidateVM(vm.ShippingAddressVM);
            }
            validationSuccess &= ValidateAddresses(vm);
            if (!validEmail) {
                ModelState.AddModelError($"{nameof(vm.Email)}", T("E-mail is invalid.").Text);
            }
            return validEmail && validationSuccess;
        }
        private bool ValidateVM(AddressEditViewModel vm) {
            int id = -1;
            if (vm.CityId > 0) {
                if (int.TryParse(vm.City, out id)) {
                    // the form sent the city's id instead of its name
                    vm.City = _addressConfigurationService
                        .GetCity(vm.CityId)
                        ?.As<TitlePart>()
                        ?.Title
                        ?? string.Empty;
                }
            }
            if (vm.ProvinceId > 0) {
                if (int.TryParse(vm.Province, out id)) {
                    // the form sent the city's id instead of its name
                    vm.Province = _addressConfigurationService
                        .GetProvince(vm.ProvinceId)
                        ?.As<TitlePart>()
                        ?.Title
                        ?? string.Empty;
                }
            }
            if (vm.CountryId > 0) {
                if (int.TryParse(vm.Country, out id)) {
                    // the form sent the city's id instead of its name
                    vm.Country = _addressConfigurationService
                        .GetCountry(vm.CountryId)
                        ?.As<TitlePart>()
                        ?.Title
                        ?? string.Empty;
                }
            }
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
        private void FinalizeVM(CheckoutViewModel vm) {
            if (vm.ShippingAddressVM == null || vm.BillingAddressVM == null) {
                return;
            }
            if (
            vm.ShippingAddressVM.CountryId == vm.BillingAddressVM.CountryId &&
            vm.ShippingAddressVM.Country == vm.BillingAddressVM.Country &&
            vm.ShippingAddressVM.CityId == vm.BillingAddressVM.CityId &&
            vm.ShippingAddressVM.City == vm.BillingAddressVM.City &&
            vm.ShippingAddressVM.Company == vm.BillingAddressVM.Company &&
            vm.ShippingAddressVM.Address1 == vm.BillingAddressVM.Address1 &&
            vm.ShippingAddressVM.Address2 == vm.BillingAddressVM.Address2 &&
            vm.ShippingAddressVM.FirstName == vm.BillingAddressVM.FirstName &&
            vm.ShippingAddressVM.LastName == vm.BillingAddressVM.LastName &&
            vm.ShippingAddressVM.Honorific == vm.BillingAddressVM.Honorific &&
            vm.ShippingAddressVM.PostalCode == vm.BillingAddressVM.PostalCode &&
            vm.ShippingAddressVM.ProvinceId == vm.BillingAddressVM.ProvinceId &&
            vm.ShippingAddressVM.Province == vm.BillingAddressVM.Province
          ) {
                vm.BillAtSameShippingAddress = true;
            }
            else {
                vm.BillAtSameShippingAddress = false;
            }
        }
    }
}