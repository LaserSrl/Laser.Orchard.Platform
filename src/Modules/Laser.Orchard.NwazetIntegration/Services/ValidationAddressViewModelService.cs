using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class ValidationAddressViewModelService : IValidationProvider {
        private readonly IAddressConfigurationService _addressConfigurationService;
        private readonly ISiteService _siteService;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        public ValidationAddressViewModelService(
            IAddressConfigurationService addressConfigurationService,
            ISiteService siteService,
            ICacheManager cacheManager,
            ISignals signals) {

            _addressConfigurationService = addressConfigurationService;
            _siteService = siteService;
            _cacheManager = cacheManager;
            _signals = signals;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        /// <summary>
        /// validation of the vm coming from a create/edit action
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        /// <remarks>
        /// It would be cleaner to do this in its own validation classes,
        /// but we need a bunch of IDependencies, so putting this code
        /// here is less of an hassle.
        /// </remarks>
        public bool Validate(AddressEditViewModel vm) {
            var validCountries = _addressConfigurationService
                .GetAllCountries(vm.AddressType);
            var countryTP = _addressConfigurationService
                .GetCountry(vm.CountryId);
            if (!SubValidation(validCountries, countryTP)) {
                return false;
            }
            var validProvinces = _addressConfigurationService
                .GetAllProvinces(vm.AddressType, countryTP);
            if (validProvinces == null) {
                // error condition
                return false;
            }
            var provinceTP = GetTerritory(vm.Province)
                ?? _addressConfigurationService.SingleTerritory(vm.ProvinceId);
            if (validProvinces.Any()) {
                // there may not be any configured province, and that is ok,
                // but if any is configured, we check that the one selected is valid
                if (!SubValidation(validProvinces, provinceTP)) {
                    return false;
                }
            }
            if (provinceTP == null) {
                // maybe we did not find a territory because it's not configured,
                // but we had a free text input for the province
                var provinceName = vm.Province.Trim();
                if (provinceName.Length < 2) {
                    // at least two characters
                    return false;
                }
            }
            var validCities = _addressConfigurationService
                .GetAllCities(vm.AddressType, 
                    // use province if it exists, otherwise country
                    provinceTP == null ? countryTP : provinceTP);
            if (validCities == null) {
                // error condition
                return false;
            }
            if (validCities.Any()) {
                // there may not be any configured city, and that is ok,
                // but if any is configured, we check that the one selected is valid
                var cityTP = GetTerritory(vm.City)
                    ?? _addressConfigurationService.SingleTerritory(vm.CityId);
                if (!SubValidation(validCities, cityTP)) {
                    return false;
                }
            }
            // TODO: zip code validation depends on the country:
            // https://en.wikipedia.org/wiki/List_of_postal_codes
            // as a first step, we want zipcode to be non empty and all digits.
            // This is not correct because there are some territories that have
            // letters in theri zip codes. We will fix this when the time comes.
            if (string.IsNullOrWhiteSpace(vm.PostalCode)) {
                return false;
            } else {
                foreach (char c in vm.PostalCode) {
                    if (c < '0' || c > '9') {
                        return false;
                    }
                }
            }
            return true;
        }
        private bool SubValidation(IEnumerable<TerritoryPart> list, TerritoryPart item) {
            if (list == null || !list.Any()) {
                return false;
            }
            if (item == null) {
                return false;
            }
            if (!list.Any(c => c.Id == item.Id)) {
                return false;
            }
            return true;
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

        public List<LocalizedString> Validate(AddressesVM vm) {
            var error = new List<LocalizedString>();
            if (PhoneNumberRequired && string.IsNullOrWhiteSpace(vm.Phone)) {
                error.Add(T("Phone number is required."));
            } else if (!string.IsNullOrWhiteSpace(vm.Phone)) {
                // validate format for phone number
                foreach (char c in vm.Phone) {
                    if (c < '0' || c > '9') {
                        error.Add(T("Phone number may contain only digits."));
                        break;
                    }
                }
            }
            if (PhoneNumberRequired && string.IsNullOrWhiteSpace(vm.PhonePrefix)) {
                error.Add(T("Phone number prefix is required."));
            } else if (!string.IsNullOrWhiteSpace(vm.PhonePrefix)) {
                // TODO: PhonePrefix should be one from a list of valid international prefixes
                if (!vm.PhonePrefix.StartsWith("+")) {
                    error.Add(T("Format for phone prefix is the + sign followed by digits."));
                } else {
                    var pp = vm.PhonePrefix.TrimStart(new char[] { '+' });
                    foreach (char c in pp) {
                        if (c < '0' || c > '9') {
                            error.Add(T("Format for phone prefix is the + sign followed by digits."));
                            break;
                        }
                    }
                }
            }
            return error;
        }

        private bool PhoneNumberRequired {
            get { return Settings.PhoneIsRequired; }
        }

        #region cache keys
        private const string _settingsCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.ValidationAddressViewModelService.Settings";
        #endregion

        private CheckoutSettingsPart Settings {
            get {
                return GetFromCache(_settingsCacheKey, () => {
                    return _siteService.GetSiteSettings()
                        .As<CheckoutSettingsPart>();
                });
            }
        }
        private T GetFromCache<T>(string cacheKey, Func<T> method) {
            return _cacheManager.Get(cacheKey, true, ctx => {
                // invalidation signal 
                ctx.Monitor(_signals.When(Constants.CheckoutSettingsCacheEvictSignal));
                // cache
                return method();
            });
        }
    }
}