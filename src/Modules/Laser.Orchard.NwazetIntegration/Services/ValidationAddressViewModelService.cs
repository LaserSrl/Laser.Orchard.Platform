using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class ValidationAddressViewModelService : IValidationProvider {
        private readonly IAddressConfigurationService _addressConfigurationService;
        public ValidationAddressViewModelService(
            IAddressConfigurationService addressConfigurationService) {
            _addressConfigurationService = addressConfigurationService;
        }

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
            var provinceTP = GetTerritory(vm.Province);
            if (validProvinces.Any()) {
                // there may not be any configured province, and that is ok,
                // but if any is configured, we check that the one selected is valid
                if (!SubValidation(validProvinces, provinceTP)) {
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
                var cityTP = GetTerritory(vm.City);
                if (!SubValidation(validCities, cityTP)) {
                    return false;
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
            return new List<LocalizedString>();
        }
    }
}