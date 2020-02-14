using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class AddressOrderPartDriver : ContentPartDriver<AddressOrderPart> {
        private readonly IAuthorizer _authorizer;
        private readonly IAddressConfigurationService _addressConfigurationService;
        public AddressOrderPartDriver(
            IAuthorizer authorizer,
            IAddressConfigurationService addressConfigurationService) {

            _authorizer = authorizer;
            _addressConfigurationService = addressConfigurationService;

            T = NullLocalizer.Instance;
        }

        protected override string Prefix {
            get { return "AddressOrderPart"; }
        }
        public Localizer T { get; set; }

        protected override DriverResult Editor(AddressOrderPart part, dynamic shapeHelper) {
            if (!Authorized(part)) {
                return null;
            }
            return ContentShape("Parts_Order_AdvancedAddress_Edit", () => {

                return shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Order.AdvancedAddress",
                    Model: CreateVM(part), //TODO
                    Prefix: Prefix);

            });
        }

        protected override DriverResult Editor(AddressOrderPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!Authorized(part)) {
                return null;
            }
            return Editor(part, shapeHelper);
        }

        private bool Authorized(AddressOrderPart part) {
            return
                // check the permission
                _authorizer.Authorize(OrderPermissions.ManageOrders, null, T("Cannot manage orders"))
                // we need to also have an OrderPart for this to work
                && part.Is<OrderPart>();
        }

        private OrderAddressEditorViewModel CreateVM(AddressOrderPart part) {
            var orderPart = part.As<OrderPart>();
            return new OrderAddressEditorViewModel {
                AddressOrderPart = part,
                OrderPart = orderPart,
                ShippingAddressVM = CreateVM(MockRecord(part, AddressRecordType.ShippingAddress)),
                BillingAddressVM = CreateVM(MockRecord(part, AddressRecordType.BillingAddress)),
            };
        }

        private AddressRecord MockRecord(AddressOrderPart part, AddressRecordType addressType) {
            var address = part.As<OrderPart>().ShippingAddress;
            var city = part.ShippingCityName;
            var province = part.ShippingProvinceName;
            var country = part.ShippingCountryName;
            var countryId = part.ShippingCountryId;
            var cityId = part.ShippingCityId;
            var provinceId = part.ShippingProvinceId;
            if (addressType == AddressRecordType.BillingAddress) {
                address = part.As<OrderPart>().BillingAddress;
                city = part.BillingCityName;
                province = part.BillingProvinceName;
                country = part.BillingCountryName;
                countryId = part.BillingCountryId;
                cityId = part.BillingCityId;
                provinceId = part.BillingProvinceId;
            }

            return new AddressRecord {
                AddressType = addressType,
                Honorific = address.Honorific,
                FirstName = address.FirstName,
                LastName = address.LastName,
                Company = address.Company,
                Address1 = address.Address1,
                Address2 = address.Address2,
                City = city,
                Province = province,
                PostalCode = address.PostalCode,
                Country = country,
                CountryId = countryId,
                CityId = cityId,
                ProvinceId = provinceId
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
                Countries = _addressConfigurationService.CountryOptions(countryId),
                CountryId = countryId
            };
        }
    }
}