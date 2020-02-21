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

        public AddressConfigurationSiteSettingsPartDriver(
            IContentManager contentManager,
            IAddressConfigurationService addressConfigurationService,
            ITerritoriesRepositoryService territoriesRepositoryService) {

            _contentManager = contentManager;
            _addressConfigurationService = addressConfigurationService;
            _territoriesRepositoryService = territoriesRepositoryService;
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
            // details configuration
            ExportTerritoryElements(root, new TerritoryExportContext {
                Ids = part.SelectedCountries,
                ArrayName = "SelectedCountries",
                ElementName = "SelectedCountry"
            });
            ExportTerritoryElements(root, new TerritoryExportContext {
                Ids = part.SelectedProvinces,
                ArrayName = "SelectedProvinces",
                ElementName = "SelectedProvince"
            });
            ExportTerritoryElements(root, new TerritoryExportContext {
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
        private void ExportTerritoryElements(XElement root, TerritoryExportContext context) {
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
            // details configuration
            part.SerializedSelectedCountries =
                JsonConvert.SerializeObject(ImportTerritoryElements(root, new TerritoryExportContext {
                    ArrayName = "SelectedCountries",
                    ElementName = "SelectedCountry"
                }));
            part.SerializedSelectedProvinces =
                JsonConvert.SerializeObject(ImportTerritoryElements(root, new TerritoryExportContext {
                    ArrayName = "SelectedProvinces",
                    ElementName = "SelectedProvince"
                }));
            part.SerializedSelectedCities =
                JsonConvert.SerializeObject(ImportTerritoryElements(root, new TerritoryExportContext {
                    ArrayName = "SelectedCities",
                    ElementName = "SelectedCity"
                }));
            // country Codes
            part.SerializedCountryCodes =
                JsonConvert.SerializeObject(root
                    .Element("CountryCodes")
                    .Elements("CountryCode")
                    .Select(xel => {
                        // read territory name and iso code
                        var name = xel.Attr("TerritoryName");
                        var code = xel.Attr("ISOCode");
                        // configured hierarchy may not be set yet, so we should
                        // just get the TerritoryInternalRecord itself
                        var tir = _territoriesRepositoryService.GetTerritoryInternal(name);
                        return new CountryAlpha2 {
                            TerritoryId = tir != null ? tir.Id : 0,
                            ISOCode = code
                        };
                    })
                    .Where(cc => cc.TerritoryId > 0)
                    .ToArray());
        }
        private int[] ImportTerritoryElements(XElement root, TerritoryExportContext context) {
            var arrayElement = root.Element(context.ArrayName);
            return arrayElement
                .Elements(context.ElementName)
                .Select(xel => {
                    // read territory name
                    var name = xel.Attr("Name");
                    // configured hierarchy may not be set yet, so we should
                    // just get the TerritoryInternalRecord itself
                    var tir = _territoriesRepositoryService.GetTerritoryInternal(name);
                    return tir != null ? tir.Id : 0;
                })
                .Where(i => i > 0)
                .ToArray();
        }
    }
}
