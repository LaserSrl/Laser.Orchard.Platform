using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;

namespace Laser.Orchard.NwazetIntegration.Services.CheckoutShippingAddressProviders {
    public class DefaultCheckoutShippingAddressProvider
        : BaseCheckoutShippingAddressProvider {
        
        private readonly IAddressConfigurationService _addressConfigurationService;
        private readonly IEnumerable<IValidationProvider> _validationProviders;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly INwazetCommunicationService _nwazetCommunicationService;
        private readonly IContentManager _contentManager;

        public DefaultCheckoutShippingAddressProvider(
            IAddressConfigurationService addressConfigurationService,
            IEnumerable<IValidationProvider> validationProviders,
            IWorkContextAccessor workContextAccessor,
            INwazetCommunicationService nwazetCommunicationService,
            IContentManager contentManager) 
            : base() {

            _addressConfigurationService = addressConfigurationService;
            _validationProviders = validationProviders;
            _workContextAccessor = workContextAccessor;
            _nwazetCommunicationService = nwazetCommunicationService;
            _contentManager = contentManager;
        }

        private const string ProviderId = "default";

        public override IEnumerable<AdditionalIndexShippingAddressViewModel> 
            GetIndexShippingAddressShapes(CheckoutViewModel cvm) {

            var user = _workContextAccessor.GetContext().CurrentUser;
            if (user != null) {
                cvm.ListAvailableShippingAddress =
                    _nwazetCommunicationService.GetShippingByUser(user);

            }

            // if there is no ShippingAddressVM, create a new one
            if (cvm.ShippingAddressVM == null) {
                cvm.ShippingAddressVM = AddressEditViewModel
                    .CreateVM(
                        _addressConfigurationService, 
                        AddressRecordType.ShippingAddress, 
                        cvm.ShippingAddressVM);
            }
            // make sure the list of coutries is populated (it may not be if
            // the VM has just been reinflated)
            if (cvm.ShippingAddressVM.Countries == null) {
                cvm.ShippingAddressVM.Countries = _addressConfigurationService
                    .CountryOptions();
                cvm.ShippingAddressVM.ShippingCountries = _addressConfigurationService
                    .CountryOptions(
                        AddressRecordType.ShippingAddress, 
                        cvm.ShippingAddressVM.CountryId);
            }

            // TODO: use the same logic for a custom shape rather than the
            // one from the index
            yield break;
        }

        public override bool IsSelectedProviderForIndex(string providerId) {
            // This is the default provider, so if for some reason no provider
            // appears selected, this will try and act as a fallback.
            return string.IsNullOrWhiteSpace(providerId)
                || ProviderId
                    .Equals(providerId, StringComparison.OrdinalIgnoreCase);
        }

        public override bool ProcessAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context, CheckoutViewModel cvm) {

            if (IsSelectedProviderForIndex(cvm.SelectedShippingAddressProviderId)) {
                return context.CheckoutController.TryUpdateModel(cvm.ShippingAddressVM);
            } else {
                return true;
            }
        }

        public override bool ValidateAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context, CheckoutViewModel cvm) {
            return ValidateVM(cvm.ShippingAddressVM);
        }

        public override int GetShippingCountryId(CheckoutViewModel cvm) {
            // TODO: see if we can refactor the viewmodel further
            if (cvm.ShippingAddressVM != null) {
                return cvm.ShippingAddressVM.CountryId;
            }
            return base.GetShippingCountryId(cvm);
        }
        public override int GetShippingProvinceId(CheckoutViewModel cvm) {
            // TODO: see if we can refactor the viewmodel further
            if (cvm.ShippingAddressVM != null) {
                return cvm.ShippingAddressVM.ProvinceId;
            }
            return base.GetShippingProvinceId(cvm);
        }
        public override int GetShippingCityId(CheckoutViewModel cvm) {
            // TODO: see if we can refactor the viewmodel further
            if (cvm.ShippingAddressVM != null) {
                return cvm.ShippingAddressVM.CityId;
            }
            return base.GetShippingCityId(cvm);
        }
        public override string GetShippingCountryName(CheckoutViewModel cvm) {
            if (cvm.ShippingAddressVM != null
                && !string.IsNullOrWhiteSpace(cvm.ShippingAddressVM.Country)) {
                return cvm.ShippingAddressVM.Country;
            }
            var countryId = GetShippingCountryId(cvm);
            if (countryId > 0) {
                var country = _addressConfigurationService
                    ?.GetCountry(countryId);
                if (country != null) {
                    return _contentManager.GetItemMetadata(country).DisplayText;
                }
            }

            return base.GetShippingCountryName(cvm);
        }
        public override string GetShippingPostalCode(CheckoutViewModel cvm) {
            // TODO: see if we can refactor the viewmodel further
            if (cvm.ShippingAddressVM != null) {
                return cvm.ShippingAddressVM.PostalCode;
            }
            return base.GetShippingPostalCode(cvm);
        }

        public override void ReinflateShippingAddress(ShippingAddressReinflationContext context) {
            if (context.TargetCheckoutViewModel.ShippingAddressVM == null) {
                context.TargetCheckoutViewModel.ShippingAddressVM = context.SourceCheckoutViewModel.ShippingAddressVM;
            }
            if (context.TargetCheckoutViewModel.ShippingAddressVM != null) {
                if (context.TargetCheckoutViewModel.ShippingAddress == null) {
                    context.TargetCheckoutViewModel.ShippingAddress = 
                        context.TargetCheckoutViewModel.ShippingAddressVM.MakeAddressFromVM();
                }

                Func<string, int, string> inflateName = (str, id) => {
                    if (string.IsNullOrWhiteSpace(str)) {
                        var territory = _addressConfigurationService
                            .SingleTerritory(id);
                        if (territory != null) {
                            return _contentManager
                                .GetItemMetadata(territory).DisplayText;
                        }
                    }
                    return str;
                };
                // reinflate the names of country, province and city
                context.TargetCheckoutViewModel.ShippingAddressVM.Country = inflateName(
                    context.TargetCheckoutViewModel.ShippingAddressVM.Country,
                    context.TargetCheckoutViewModel.ShippingAddressVM.CountryId);
                context.TargetCheckoutViewModel.ShippingAddressVM.Province = inflateName(
                    context.TargetCheckoutViewModel.ShippingAddressVM.Province, 
                    context.TargetCheckoutViewModel.ShippingAddressVM.ProvinceId);
                context.TargetCheckoutViewModel.ShippingAddressVM.City = inflateName(
                    context.TargetCheckoutViewModel.ShippingAddressVM.City, 
                    context.TargetCheckoutViewModel.ShippingAddressVM.CityId);
            }
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

    }
}