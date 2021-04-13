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
        private readonly IAddressConfigurationService _addressConfigurationService;
        private readonly IEnumerable<IValidationProvider> _validationProvider;
        private readonly ICheckoutHelperService _checkoutHelperService;

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
            IAddressConfigurationService addressConfigurationService,
            IEnumerable<IValidationProvider> validationProvider,
            ICheckoutHelperService checkoutHelperService) {

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
            _addressConfigurationService = addressConfigurationService;
            _validationProvider = validationProvider;
            _checkoutHelperService = checkoutHelperService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        
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
        public ActionResult Create(int type = 0) {
            var user = _workContextAccessor.GetContext().CurrentUser;
            if (user == null) {
                // we should never be here, because the AuthorizeAttribute should
                // take care of anonymous users.
                return new HttpUnauthorizedResult(T("Sign In to  manage your saved addresses.").Text);
            }
            var model = CreateVM();
            model.AddressType = type == 0 ? AddressRecordType.ShippingAddress : AddressRecordType.BillingAddress;
            return View(model);
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

                FixUpdate(newAddress);

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

                FixUpdate(newAddress);

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
        [HttpGet]
        public JsonResult GetAdministrativeInfo(int territoryId) {
            var country = _addressConfigurationService.GetCountry(territoryId);
            var part = country?.As<TerritoryAdministrativeTypePart>();
            if (part != null) {
                return Json(new { part.HasCities, part.HasProvinces, part.AdministrativeType }, JsonRequestBehavior.AllowGet);
            }
            else {
                return null;
            }
        }


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
                if (viewModel.CityId != 0) {
                    var selectedCity = _addressConfigurationService.GetCity(viewModel.CityId);
                    if (selectedCity != null && !cities.Any(c => c.Record.TerritoryInternalRecord.Id == viewModel.CityId)) {
                        cities = cities.Concat(new[] { selectedCity });
                    }
                }
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


        [HttpGet]
        [ActionName("citiesapi")]
        public JsonResult GetCities(string query, int countryId, bool isBillingAddress) {
            var country = _addressConfigurationService.GetCountry(countryId);
            if (country != null) {
                var cities = _addressConfigurationService.GetAllCities(
                    isBillingAddress
                        ? AddressRecordType.BillingAddress
                        : AddressRecordType.ShippingAddress,
                    country,
                    query);

                return Json(cities.Select(tp => new TerritoryTag {
                    Label = tp.As<TitlePart>().Title,
                    Value = tp.Record.TerritoryInternalRecord.Id.ToString()
                }), JsonRequestBehavior.AllowGet);
            }
            return Json(new List<TerritoryTag>(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ActionName("provincesapi")]
        public JsonResult GetProvincesGet(string query, int countryId, int cityId, string cityName, bool isBillingAddress) {
            var jsonResult = GetProvincesPost(new ConfigurationRequestViewModel {
                CountryId = countryId,
                CityId = cityId,
                CityName = cityName,
                IsBillingAddress = isBillingAddress
            });
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
        [HttpPost]
        [ActionName("GetProvinces")]
        public JsonResult GetProvincesPost(ConfigurationRequestViewModel viewModel) {
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
            return AddressEditViewModel.CreateVM(_addressConfigurationService);
        }
        private AddressEditViewModel CreateVM(AddressRecordType addressRecordType) {
            return AddressEditViewModel.CreateVM(_addressConfigurationService, addressRecordType);
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

            var cityId = address.CityId;
            if (cityId <= 0 && !string.IsNullOrWhiteSpace(address.City)) {
                var tp = _addressConfigurationService.GetCity(address.City);
                if (tp != null) {
                    cityId = tp.Record.TerritoryInternalRecord.Id;
                }
            }

            var provinceId = address.ProvinceId;
            if (provinceId <= 0 && !string.IsNullOrWhiteSpace(address.Province)) {
                var tp = _addressConfigurationService.GetProvince(address.Province);
                if (tp != null) {
                    provinceId = tp.Record.TerritoryInternalRecord.Id;
                }
            }

            return new AddressEditViewModel(address) {
                Countries = _addressConfigurationService
                    .CountryOptions(address.AddressType, countryId),
                ShippingCountries = _addressConfigurationService.CountryOptions(AddressRecordType.ShippingAddress),
                BillingCountries = _addressConfigurationService.CountryOptions(AddressRecordType.BillingAddress),
                CountryId = countryId,
                CityId = cityId,
                ProvinceId = provinceId
            };
        }

        private bool ValidateVM(AddressEditViewModel vm) {
            bool response = true;
            foreach (var valP in _validationProvider) {
                if (!valP.Validate(vm)) {
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
                    ModelState.AddModelError("_FORM", error);
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
            // City: we may be settings either the Id or the string
            if (vm.CityId > 0) {
                var cityTP = GetTerritory(vm.CityId.ToString());
                if (cityTP != null) {
                    vm.CityId = cityTP.Record.TerritoryInternalRecord.Id;
                    vm.City = _contentManager.GetItemMetadata(cityTP).DisplayText;
                }
                else {
                    vm.CityId = -1;
                    vm.City = "";
                }
            }
            else {
                vm.CityId = -1;
            }
            // Province: we may be settings either the Id or the string
            if (vm.ProvinceId > 0) {
                var provinceTP = GetTerritory(vm.ProvinceId.ToString());
                if (provinceTP != null) {
                    vm.ProvinceId = provinceTP.Record.TerritoryInternalRecord.Id;
                    vm.Province = _contentManager.GetItemMetadata(provinceTP).DisplayText;
                }
                else {
                    vm.ProvinceId = -1;
                    vm.Province = "";
                }
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