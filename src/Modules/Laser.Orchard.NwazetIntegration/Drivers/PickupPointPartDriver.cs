using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Security;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointPartDriver
        : ContentPartDriver<PickupPointPart> {
        private readonly IAuthorizer _authorizer;
        private readonly IAddressConfigurationService _addressConfigurationService;

        public PickupPointPartDriver(
            IAuthorizer authorizer,
            IAddressConfigurationService addressConfigurationService) {

            _authorizer = authorizer;
            _addressConfigurationService = addressConfigurationService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "PickupPointPart"; }
        }


        protected override DriverResult Editor(PickupPointPart part, dynamic shapeHelper) {
            if (!Authorized(part)) {
                return null;
            }
            return ContentShape("Parts_PickupPointPart_Edit", () => {
                return shapeHelper.EditorTemplate(
                    TemplateName: "Parts/PickupPoint",
                    Model: CreateVM(part),
                    Prefix: Prefix);
            });
        }

        protected override DriverResult Editor(PickupPointPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!Authorized(part)) {
                return null;
            }
            var updatedModel = new PickupPointPartEditViewModel();

            var updateIsValid = updater.TryUpdateModel(updatedModel, Prefix, null, null);
            updateIsValid &= ValidateVM(updatedModel.AddressVM);
            if (updateIsValid) {
                part.AddressLine1 = updatedModel.AddressVM.Address1;
                part.AddressLine2 = updatedModel.AddressVM.Address2;
                part.CountryId = updatedModel.AddressVM.CountryId;
                part.CountryName = updatedModel.AddressVM.Country;
                part.ProvinceId = updatedModel.AddressVM.ProvinceId;
                part.ProvinceName = updatedModel.AddressVM.Province;
                part.CityId = updatedModel.AddressVM.CityId;
                part.CityName = updatedModel.AddressVM.City;
            }

            return Editor(part, shapeHelper);
        }

        private bool ValidateVM(PickupPointAddressEditViewModel vm) {
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
            return true;
        }

        private bool Authorized(PickupPointPart part) {
            return _authorizer.Authorize(
                PickupPointPermissions.MayConfigurePickupPoints, 
                part, 
                T("Cannot manage pickup points"));
        }

        private PickupPointPartEditViewModel CreateVM(PickupPointPart part) {

            return new PickupPointPartEditViewModel {
                PickupPointPart = part,
                AddressVM = CreateAddressVM(part)
            };
        }

        private PickupPointAddressEditViewModel CreateAddressVM(PickupPointPart part) {
            
            return new PickupPointAddressEditViewModel(part) {
                Countries = _addressConfigurationService
                    .CountryOptions(AddressRecordType.ShippingAddress, part.CountryId)
            };
        }
    }
}