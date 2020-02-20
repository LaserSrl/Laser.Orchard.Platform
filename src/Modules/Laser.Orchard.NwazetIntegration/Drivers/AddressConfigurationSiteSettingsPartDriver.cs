using Laser.Orchard.NwazetIntegration.Controllers;
using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Newtonsoft.Json;
using Nwazet.Commerce.Controllers;
using Nwazet.Commerce.Models;
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

        public AddressConfigurationSiteSettingsPartDriver(
            IContentManager contentManager,
            IAddressConfigurationService addressConfigurationService) {

            _contentManager = contentManager;
            _addressConfigurationService = addressConfigurationService;
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

        protected override void Exporting(
            AddressConfigurationSiteSettingsPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            // base configuration
            var hierarchy = _contentManager
                .Get<TerritoryHierarchyPart>(part.ShippingCountriesHierarchyId);
            var hierarchyIdentity = _contentManager
                .GetItemMetadata(hierarchy).Identity;
            root.SetAttributeValue("ShippingCountriesHierarchyId", hierarchyIdentity);
            // details configuration
            AddTerritoryElements(root, new TerritoryExportContext {
                Ids = part.SelectedCountries,
                ArrayName = "SelectedCountries",
                ElementName = "SelectedCountry"
            });
            AddTerritoryElements(root, new TerritoryExportContext {
                Ids = part.SelectedProvinces,
                ArrayName = "SelectedProvinces",
                ElementName = "SelectedProvince"
            });
            AddTerritoryElements(root, new TerritoryExportContext {
                Ids = part.SelectedCities,
                ArrayName = "SelectedCities",
                ElementName = "SelectedCity"
            });
            // Country codes
            root.AddEl(new XElement("CountryCodes")
                .AddEl(part.CountryCodes
                    .Select(cc => {
                        var ele = new XElement("CountryCode");
                        ele.SetAttributeValue("TerritoryName",
                            _addressConfigurationService
                                .SingleTerritory(cc.TerritoryId)
                                ?.Record.TerritoryInternalRecord.Name ?? string.Empty);
                        ele.SetAttributeValue("ISOCode",
                            cc.ISOCode);
                        return ele;
                    })
                    .ToArray()));
        }

        private void AddTerritoryElements(XElement root, TerritoryExportContext context) {
            // our int[] should be exported as strings for the names of the
            // TerritoryInternalRecords
            var tNames = context.Ids
                .Select(tirid => _addressConfigurationService
                    .SingleTerritory(tirid)
                    ?.Record.TerritoryInternalRecord.Name ?? string.Empty)
                .Where(cn => !string.IsNullOrWhiteSpace(cn));
            // for these arrays that represent TerritoryInternalRecord[], we
            // do an xml element, with a child element for each territory.
            // The reason is that in exporting/importing TerritoryInternalRecords
            // we use their names rather than their ids, and it's non-trivial to
            // be able to split them properly in an attribute.
            root
                .AddEl(new XElement(context.ArrayName)
                    .AddEl(
                        tNames
                            .Select(cn => {
                                var ele = new XElement(context.ElementName);
                                ele.SetAttributeValue("Name", cn);
                                return ele;
                            })
                            .ToArray()));
        }
        class TerritoryExportContext {
            public int[] Ids { get; set; }
            public string ArrayName { get; set; }
            public string ElementName { get; set; }
        }
        protected override void Importing(
            AddressConfigurationSiteSettingsPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            // Don't do anything if the tag is not specified.
            if (root == null) {
                return;
            }

            // base configuration
            context.ImportAttribute(
                part.PartDefinition.Name,
                "ShippingCountriesHierarchyId",
                identity =>
                    part.ShippingCountriesHierarchyId = context.GetItemFromSession(identity).Id);
            // details configuration
            
        }
    }
}
