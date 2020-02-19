using Laser.Orchard.NwazetIntegration.Controllers;
using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Newtonsoft.Json;
using Nwazet.Commerce.Controllers;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class AddressConfigurationSiteSettingsPartDriver
        : ContentPartDriver<AddressConfigurationSiteSettingsPart> {

        private readonly IContentManager _contentManager;

        public AddressConfigurationSiteSettingsPartDriver(
            IContentManager contentManager) {

            _contentManager = contentManager;
        }

        protected override string Prefix {
            get { return "AddressConfigurationSiteSettings"; }
        }

        protected override DriverResult Editor(
            AddressConfigurationSiteSettingsPart part, dynamic shapeHelper) {

            var baseShape = ContentShape("SiteSettings_AddressConfiguration",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "SiteSettings/AddressConfiguration",
                    Model: CreateBaseVM(part),
                    Prefix: Prefix)
                ).OnGroup("ECommerceSiteSettings");

            var detailShape = ContentShape("SiteSettings_AddressConfiguration_Details",
                () => {
                    if (part.ShippingCountriesHierarchyId <= 0
                        || _contentManager.Get<TerritoryHierarchyPart>(part.ShippingCountriesHierarchyId) == null) {
                        return shapeHelper.EditorTemplate(
                            TemplateName: "SiteSettings/AddressConfigurationDetailsMissing",
                            Model: null,
                            Prefix: Prefix);
                    }
                    return shapeHelper.EditorTemplate(
                        TemplateName: "SiteSettings/AddressConfigurationDetails",
                        Model: CreateDetailVM(part),
                        Prefix: Prefix);
                }
                ).OnGroup("AddressConfigurationSiteSettings");

            return Combined(
                baseShape,
                detailShape
                );
        }

        protected override DriverResult Editor(
            AddressConfigurationSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            if (updater is ECommerceSettingsAdminController) {
                // base settings
                var model = new AddressConfigurationSiteSettingsPartViewModel();
                if (updater.TryUpdateModel(model, Prefix, null, null)) {
                    if (part.ShippingCountriesHierarchyId != model.ShippingCountriesHierarchyId) {
                        // selected hierarchy changed, so we need to reset
                        // the selected territories
                        part.ResetDetails();
                    }
                    // selected hierarchy
                    part.ShippingCountriesHierarchyId = model.ShippingCountriesHierarchyId;
                }
            } else if (updater is AddressConfigurationAdminController) {
                // detailed settings
                var model = new AddressConfigurationSiteSettingsPartViewModel();
                if (updater.TryUpdateModel(model, Prefix, null, null)) {
                    // TODO
                    // model.TerritoryTypeMap will contain the desired configuration
                    part.SerializedSelectedCountries =
                        JsonConvert.SerializeObject(GetCountries(model));
                    part.SerializedSelectedProvinces =
                        JsonConvert.SerializeObject(GetProvinces(model));
                    part.SerializedSelectedCities =
                        JsonConvert.SerializeObject(GetCities(model));
                    // model.TerritoryISOCode contains the iso codes
                    part.SerializedCountryCodes =
                        JsonConvert.SerializeObject(GetISOCodes(model));
                }
            }
            return Editor(part, shapeHelper);
        }

        private IEnumerable<int> GetCountries(AddressConfigurationSiteSettingsPartViewModel vm) {
            return GetTerritoryIdsByType(vm, TerritoryTypeForAddress.Country);
        }
        private IEnumerable<int> GetProvinces(AddressConfigurationSiteSettingsPartViewModel vm) {
            return GetTerritoryIdsByType(vm, TerritoryTypeForAddress.Province);
        }
        private IEnumerable<int> GetCities(AddressConfigurationSiteSettingsPartViewModel vm) {
            return GetTerritoryIdsByType(vm, TerritoryTypeForAddress.City);
        }
        private IEnumerable<int> GetTerritoryIdsByType(
            AddressConfigurationSiteSettingsPartViewModel vm, TerritoryTypeForAddress type) {
            return vm.TerritoryTypeMap
                .Where(kvp => kvp.Value == type)
                .Select(kvp => kvp.Key);
        }

        private IEnumerable<CountryAlpha2> GetISOCodes(AddressConfigurationSiteSettingsPartViewModel vm) {
            return vm.TerritoryISOCode
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value)
                    && kvp.Value.Length >= 2) // alpha-2 is 2 characters
                .Select(kvp => new CountryAlpha2 {
                    TerritoryId = kvp.Key,
                    ISOCode = kvp.Value
                    .Substring(0, 2) // alpha-2 is 2 characters
                    .ToUpperInvariant() // Capitalize for clarity
                });
        }

        private AddressConfigurationSiteSettingsPartViewModel CreateBaseVM(
            AddressConfigurationSiteSettingsPart part) {
            return new AddressConfigurationSiteSettingsPartViewModel(part) {
                AllHierarchies = _contentManager
                    .Query<TerritoryHierarchyPart>(VersionOptions.Published)
                    .List()
            };
        }

        private AddressConfigurationSiteSettingsPartViewModel CreateDetailVM(
            AddressConfigurationSiteSettingsPart part) {

            var vm = new AddressConfigurationSiteSettingsPartViewModel(part, true) {
            };

            return vm;
        }
    }
}
