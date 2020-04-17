using Laser.Orchard.NwazetIntegration.Controllers;
using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Newtonsoft.Json;
using Nwazet.Commerce.Controllers;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class AddressConfigurationSiteSettingsPartDriver
        : ContentPartDriver<AddressConfigurationSiteSettingsPart> {

        private readonly IContentManager _contentManager;
        private readonly IAddressConfigurationService _addressConfigurationService;
        private readonly ITerritoriesRepositoryService _territoriesRepositoryService;
        private readonly IAddressConfigurationSettingsService _addressConfigurationSettingsService;

        public AddressConfigurationSiteSettingsPartDriver(
            IContentManager contentManager,
            IAddressConfigurationService addressConfigurationService,
            ITerritoriesRepositoryService territoriesRepositoryService,
            IAddressConfigurationSettingsService addressConfigurationSettingsService) {

            _contentManager = contentManager;
            _addressConfigurationService = addressConfigurationService;
            _territoriesRepositoryService = territoriesRepositoryService;
            _addressConfigurationSettingsService = addressConfigurationSettingsService;
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
                    // selected hierarchy
                    part.ShippingCountriesHierarchyId = model.ShippingCountriesHierarchyId;
                }
            } else if (updater is AddressConfigurationAdminController) {
                // detailed settings
                var model = new AddressConfigurationSiteSettingsPartViewModel();
                if (updater.TryUpdateModel(model, Prefix, null, null)) {
                }
            }
            return Editor(part, shapeHelper);
        }

        private IEnumerable<int> GetCountries(AddressConfigurationSiteSettingsPartViewModel vm) {
            return GetTerritoryIdsByType(vm, TerritoryAdministrativeType.Country);
        }
        private IEnumerable<int> GetProvinces(AddressConfigurationSiteSettingsPartViewModel vm) {
            return GetTerritoryIdsByType(vm, TerritoryAdministrativeType.Province);
        }
        private IEnumerable<int> GetCities(AddressConfigurationSiteSettingsPartViewModel vm) {
            return GetTerritoryIdsByType(vm, TerritoryAdministrativeType.City);
        }
        private IEnumerable<int> GetTerritoryIdsByType(
            AddressConfigurationSiteSettingsPartViewModel vm, TerritoryAdministrativeType type) {
            return vm.TerritoryTypeMap
                .Where(kvp => kvp.Value == type)
                .Select(kvp => kvp.Key);
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

            var vm = new AddressConfigurationSiteSettingsPartViewModel(part, _addressConfigurationSettingsService) {
            };

            return vm;
        }

        protected override void Exporting(
            AddressConfigurationSiteSettingsPart part, ExportContentContext context) {
            // "normal" export for settings would export all the part's properties
            // (that are base types) in attributes of the XElement. Then it would just
            // parse them on import. However, for this instance that would break things
            // because:
            // - The ShippingCountriesHierarchyId should be handled through the corresponding
            //   ContentItem's Identity.
            // - The arrays for the territories and the ISO codes use, within the application
            //   the Id of the record, but to "map" to different tenants they should use
            //   its name.
            var root = context.Element(part.PartDefinition.Name);
            // base configuration
            var hierarchy = _contentManager
                .Get<TerritoryHierarchyPart>(part.ShippingCountriesHierarchyId);
            var hierarchyIdentity = _contentManager
                .GetItemMetadata(hierarchy).Identity;
            root.SetAttributeValue("ShippingCountriesHierarchyIdentity", hierarchyIdentity);
        }

        protected override void Imported(
            AddressConfigurationSiteSettingsPart part, ImportContentContext context) {
            // We need to do this in the Imported method, rather than the usual Importing
            // because for settings, the default executor would try to parse automatic
            // attributes after Importing and before Imported. Here we need to override
            // the results of that based on the actual information we want to be copying.
            var root = context.Data.Element(part.PartDefinition.Name);
            // Don't do anything if the tag is not specified.
            if (root == null) {
                return;
            }

            // base configuration
            context.ImportAttribute(
                part.PartDefinition.Name,
                "ShippingCountriesHierarchyIdentity",
                identity =>
                    part.ShippingCountriesHierarchyId = context.GetItemFromSession(identity).Id);
        }
    }
}
