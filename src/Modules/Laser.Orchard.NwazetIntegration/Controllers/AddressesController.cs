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
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Controllers {
    public class AddressesController : Controller, IUpdateModel {
        private readonly IOrderService _orderService;
        private readonly IShoppingCart _shoppingCart;
        private readonly IOrchardServices _orchardServices;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly INwazetCommunicationService _nwazetCommunicationService;
        private readonly IPaymentService _paymentService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ITransactionManager _transactionManager;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly IProductPriceService _productPriceService;
        private readonly IAddressConfigurationService _addressConfigurationService;
        private readonly IEnumerable<IOrderAdditionalInformationProvider> _orderAdditionalInformationProviders;
        private readonly IEnumerable<IValidationProvider> _validationProvider;

        private readonly dynamic _shapeFactory;

        public AddressesController(
            IOrderService orderService,
            IPaymentService paymentService,
            IShoppingCart shoppingCart,
            IOrchardServices orchardServices,
            ICurrencyProvider currencyProvider,
            INwazetCommunicationService nwazetCommunicationService,
            IWorkContextAccessor workContextAccessor,
            IShapeFactory shapeFactory,
            ITransactionManager transactionManager,
            IContentManager contentManager,
            INotifier notifier,
            IProductPriceService productPriceService,
            IAddressConfigurationService addressConfigurationService,
            IEnumerable<IOrderAdditionalInformationProvider> orderAdditionalInformationProviders,
            IEnumerable<IValidationProvider> validationProvider) {

            _orderService = orderService;
            _paymentService = paymentService;
            _shoppingCart = shoppingCart;
            _orchardServices = orchardServices;
            _currencyProvider = currencyProvider;
            _nwazetCommunicationService = nwazetCommunicationService;
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;
            _transactionManager = transactionManager;
            _contentManager = contentManager;
            _notifier = notifier;
            _productPriceService = productPriceService;
            _addressConfigurationService = addressConfigurationService;
            _orderAdditionalInformationProviders = orderAdditionalInformationProviders;
            _validationProvider = validationProvider;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [Themed]
        public ActionResult Index(AddressesVM model) {
            ActionResult result = null;
            var thecurrentUser = _orchardServices.WorkContext.CurrentUser;

            switch (model.Submit) {
                case "cart":
                result = RedirectToAction("Index", "ShoppingCart", new { area = "Nwazet.Commerce" });
                break;
                case "save":
                // validate addresses
                if (!TryUpdateModel(model.ShippingAddressVM) || !TryUpdateModel(model.BillingAddressVM) ||
                    !ValidateVM(model.ShippingAddressVM) || !ValidateVM(model.BillingAddressVM) ||
                    !ValidateVM(model)) {

                    // in case of error, repopulate the default lists
                    model.ShippingAddressVM = CreateVM(AddressRecordType.ShippingAddress);
                    model.BillingAddressVM = CreateVM(AddressRecordType.BillingAddress);
                    if (thecurrentUser != null) {
                        model.ListAvailableBillingAddress = _nwazetCommunicationService.GetBillingByUser(thecurrentUser);
                        model.ListAvailableShippingAddress = _nwazetCommunicationService.GetShippingByUser(thecurrentUser);
                    }
                    result = View("Index", model);
                    break;
                }
                // Hack: based on the address coming in model.ShippingAddressVM, we can compute the actual
                // destinations to be used for tax computations at this stage
                var countryName = _addressConfigurationService
                    ?.GetCountry(model.ShippingAddressVM.CountryId)
                    ?.Record?.TerritoryInternalRecord.Name;
                // costruisce la lista di CheckoutItems in base al contenuto del carrello
                List<CheckoutItem> items = new List<CheckoutItem>();
                foreach (var prod in _shoppingCart.GetProducts()) {
                    items.Add(new CheckoutItem {
                        Attributes = prod.AttributeIdsToValues,
                        LinePriceAdjustment = prod.LinePriceAdjustment,
                        OriginalPrice = prod.OriginalPrice,
                        Price = prod.Product.DiscountPrice >= 0 && prod.Product.DiscountPrice < prod.Product.Price
                            ? _productPriceService.GetDiscountPrice(prod.Product, countryName, null)
                            : _productPriceService.GetPrice(prod.Product, countryName, null),
                        ProductId = prod.Product.Id,
                        PromotionId = prod.Promotion == null ? null : (int?)(prod.Promotion.Id),
                        Quantity = prod.Quantity,
                        Title = prod.Product.ContentItem.As<TitlePart>().Title
                    });
                }
                // check if there are products in the cart
                if (items.Count > 0) {
                    var paymentGuid = Guid.NewGuid().ToString();
                    var charge = new PaymentGatewayCharge("Payment Gateway", paymentGuid);
                    // get Orchard user id
                    var userId = -1;
                    var currentUser = _orchardServices.WorkContext.CurrentUser;
                    if (currentUser != null) {
                        userId = currentUser.Id;
                    }

                    // update addresses based on those populated in the form
                    model.ShippingAddress = AddressFromVM(model.ShippingAddressVM);
                    model.BillingAddress = AddressFromVM(model.BillingAddressVM);

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
                        OrderPart.Pending, //.Cancelled,
                        null,
                        false,
                        userId,
                        0,
                        "",
                        currency);
                    // update advanced address information
                    var addressPart = order.As<AddressOrderPart>();
                    if (addressPart != null) {
                        // shipping info
                        addressPart.ShippingCountryName = model.ShippingAddressVM.Country;
                        addressPart.ShippingCountryId = model.ShippingAddressVM.CountryId;
                        addressPart.ShippingCityName = model.ShippingAddressVM.City;
                        addressPart.ShippingCityId = model.ShippingAddressVM.CityId;
                        addressPart.ShippingProvinceName = model.ShippingAddressVM.Province;
                        addressPart.ShippingProvinceId = model.ShippingAddressVM.ProvinceId;
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
                    order.LogActivity(OrderPart.Event, "Order created");
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
                    // save the addresses for the contact doing the order.
                    _nwazetCommunicationService.OrderToContact(order);
                    var reason = string.Format("Purchase Order {0}", order.OrderKey);
                    var payment = new PaymentRecord {
                        Reason = reason,
                        Amount = order.Total,
                        Currency = order.CurrencyCode,
                        ContentItemId = order.Id
                    };
                    var nonce = _paymentService.CreatePaymentNonce(payment);
                        result = RedirectToAction("Pay", "Payment",
                            new {
                                area = "Laser.Orchard.PaymentGateway",
                                nonce = nonce,
                                newPaymentGuid = paymentGuid
                            });
                }
                else {
                    _notifier.Information(T("There are no products in the cart. Go back to the catalog and add products."));
                    result = View("Index", model);
                }
                break;
                default:
                model.ShippingAddressVM = CreateVM(AddressRecordType.ShippingAddress);
                model.BillingAddressVM = CreateVM(AddressRecordType.BillingAddress);
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

        private Address AddressFromVM(AddressEditViewModel vm) {
            FixUpdate(vm);
            return new Address {
                Honorific = vm.Honorific,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Company = vm.Company,
                Address1 = vm.Address1,
                Address2 = vm.Address2,
                PostalCode = vm.PostalCode,
                // advanced address stuff
                // The string values here are the DisplayText properties of
                // configured territories, or "custom" text entered by the user.
                Country = vm.Country,
                City = vm.City,
                Province = vm.Province
            };
        }

        #region Address CRUD
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

            return View((object)_shapeFactory.ViewModel()
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

            _notifier.Information(T("That address has been removed."));
            return this.RedirectLocal(returnUrl, () => RedirectToAction("MyAddresses"));
        }

        [HttpGet, Themed, OutputCache(NoStore = true, Duration = 0), Authorize]
        public ActionResult Create() {
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (user == null) {
                // we should never be here, because the AuthorizeAttribute should
                // take care of anonymous users.
                return new HttpUnauthorizedResult(T("Sign In to  manage your saved addresses.").Text);
            }
            return View(CreateVM());
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
            if (!TryUpdateModel(newAddress) || !ValidateVM(newAddress)) {
                _transactionManager.Cancel();
                // added the assignment of the lists because in case of error in the validation the properties are not populated
                newAddress.ShippingCountries = _addressConfigurationService.CountryOptions(AddressRecordType.ShippingAddress);
                newAddress.BillingCountries = _addressConfigurationService.CountryOptions(AddressRecordType.BillingAddress);

                newAddress.Errors.Add(T("It was impossible to validate your address.").Text);
                return View(newAddress);
            }
            // Convert the values of Country, City, and Province to strings and ids for
            // the AddressRecord.
            FixUpdate(newAddress);
            // save record
            _nwazetCommunicationService.AddAddress(newAddress.AddressRecord, user);
            _notifier.Information(T("Address created successfully."));
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
            return View(CreateVM(address));
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

            if (!TryUpdateModel(newAddress) || !ValidateVM(newAddress)) {
                _transactionManager.Cancel();
                // added the assignment of the lists because in case of error in the validation the properties are not populated
                newAddress.ShippingCountries = _addressConfigurationService.CountryOptions(AddressRecordType.ShippingAddress);
                newAddress.BillingCountries = _addressConfigurationService.CountryOptions(AddressRecordType.BillingAddress);

                newAddress.Errors.Add(T("It was impossible to validate your address.").Text);
                return View(newAddress);
            }
            // Convert the values of Country, City, and Province to strings and ids for
            // the AddressRecord.
            FixUpdate(newAddress);
            // save record
            _nwazetCommunicationService.AddAddress(newAddress.AddressRecord, user);

            _notifier.Information(T("Address updated successfully."));
            return RedirectToAction("Edit", new { id = newAddress.AddressRecord.Id });
        }
        #endregion

        #region Actions for advanced address configuration

        [HttpPost]
        public JsonResult GetCities(ConfigurationRequestViewModel viewModel) {
            var country = _addressConfigurationService.GetCountry(viewModel.CountryId);
            if (country == null) {
                // this is an error
            }
            else {
                var cities = _addressConfigurationService.GetAllCities(
                    viewModel.IsBillingAddress
                        ? AddressRecordType.BillingAddress
                        : AddressRecordType.ShippingAddress,
                    country);
                return Json(new {
                    Success = true,
                    Cities = cities
                        .Select(tp =>
                            new {
                                Value = tp.Record.TerritoryInternalRecord.Id,
                                Text = _contentManager.GetItemMetadata(tp).DisplayText
                            })
                        .OrderBy(obj => obj.Text)
                });
            }
            // TODO
            return Json(new List<string>());
        }

        [HttpPost]
        public JsonResult GetProvinces(ConfigurationRequestViewModel viewModel) {
            var country = _addressConfigurationService.GetCountry(viewModel.CountryId);
            var city = string.IsNullOrWhiteSpace(viewModel.CityName)
                ? _addressConfigurationService.GetCity(viewModel.CityId)
                : _addressConfigurationService.GetCity(viewModel.CityName);
            if (country == null) {
                // this is an error
            }
            else {
                // city may be null: that is handled in the service
                var provinces = _addressConfigurationService.GetAllProvinces(
                    viewModel.IsBillingAddress
                        ? AddressRecordType.BillingAddress
                        : AddressRecordType.ShippingAddress,
                    country, city);
                return Json(new {
                    Success = true,
                    Provinces = provinces
                        .Select(tp =>
                            new {
                                Value = tp.Record.TerritoryInternalRecord.Id,
                                Text = _contentManager.GetItemMetadata(tp).DisplayText
                            })
                        .OrderBy(obj => obj.Text)
                });
            }
            // TODO
            return Json(new List<string>());
        }

        #endregion

        private AddressEditViewModel CreateVM() {
            //TODO: Handle address type correctly
            return new AddressEditViewModel() {
                Countries = _addressConfigurationService
                    .CountryOptions(),
                ShippingCountries = _addressConfigurationService
                    .CountryOptions(AddressRecordType.ShippingAddress),
                BillingCountries = _addressConfigurationService
                    .CountryOptions(AddressRecordType.BillingAddress)
            };
        }
        private AddressEditViewModel CreateVM(AddressRecordType addressRecordType) {
            return new AddressEditViewModel() {
                Countries = _addressConfigurationService.CountryOptions(addressRecordType),
                ShippingCountries = _addressConfigurationService.CountryOptions(AddressRecordType.ShippingAddress),
                BillingCountries = _addressConfigurationService.CountryOptions(AddressRecordType.BillingAddress),
                AddressType = addressRecordType
            };
        }

        private AddressEditViewModel CreateVM(AddressRecord address) {

            // defaults to "no country selected" for a new or "legacy" AddressRecord
            var countryId = address.CountryId;
            if (countryId == 0 && !string.IsNullOrWhiteSpace(address.Country)) {
                // from address.Country, find the value that should be used 
                // address.Country is of type string. It could represent the
                // name of the country (legacy) or the Id of the country territory.
                // Try to parse it.
                if (!int.TryParse(address.Country, out countryId)) {
                    // parsing failed, so the string may be a territory's name
                    var tp = _addressConfigurationService.GetCountry(address.Country);
                    if (tp != null) {
                        countryId = tp.Record.TerritoryInternalRecord.Id;
                    }
                }
            }

            return new AddressEditViewModel(address) {
                Countries = _addressConfigurationService
                    .CountryOptions(address.AddressType, countryId),
                ShippingCountries = _addressConfigurationService.CountryOptions(AddressRecordType.ShippingAddress),
                BillingCountries = _addressConfigurationService.CountryOptions(AddressRecordType.BillingAddress),
                CountryId = countryId
            };
        }

        private bool ValidateVM(AddressEditViewModel vm) {
            bool response = true;
            foreach (var valP in _validationProvider) {
                if(!valP.Validate(vm)) {
                    response = false;
                }
            }
            return response;
        }
        private bool ValidateVM(AddressesVM vm) {
            bool response = true;
            foreach (var valP in _validationProvider) {
                var result = valP.Validate(vm);
                if (result.Count() > 0) {
                    response = false;
                }
                foreach (var error in valP.Validate(vm)) {
                    ModelState.AddModelError("_FORM",error);
                } 
            }
            return response;
        }

        /// <summary>
        /// Extract a specific territory from the configured hierarchy
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private TerritoryPart GetTerritory(string value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return null;
            }
            var id = 0;
            if (int.TryParse(value, out id)) {
                return _addressConfigurationService.SingleTerritory(id);
            }
            return _addressConfigurationService.SingleTerritory(value);
        }

        private void FixUpdate(AddressEditViewModel vm) {
            // Country: front end sets the Id => we need to set the string
            var countryTP = _addressConfigurationService
                .GetCountry(vm.CountryId);
            vm.Country = _contentManager.GetItemMetadata(countryTP).DisplayText;
            // City: we may be settings either the Id or the string, but either way the
            //   property we are setting is vm.City. We get the territory and set the Id
            var cityTP = GetTerritory(vm.City);
            if (cityTP != null) {
                vm.CityId = cityTP.Record.TerritoryInternalRecord.Id;
                vm.City = _contentManager.GetItemMetadata(cityTP).DisplayText;
            }
            else {
                vm.CityId = -1;
            }
            // Province: we may be settings either the Id or the string, but either way the
            //   property we are setting is vm.Province. We get the territory and set the Id
            var provinceTP = GetTerritory(vm.Province);
            if (provinceTP != null) {
                vm.ProvinceId = provinceTP.Record.TerritoryInternalRecord.Id;
                vm.Province = _contentManager.GetItemMetadata(provinceTP).DisplayText;
            }
            else {
                vm.ProvinceId = -1;
            }
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}