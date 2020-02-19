using Laser.Orchard.NwazetIntegration.Models;
using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class AddressConfigurationSiteSettingsPartViewModel {
        public AddressConfigurationSiteSettingsPartViewModel() {
            AllHierarchies = new List<TerritoryHierarchyPart>();
            TerritoryTypeMap = new Dictionary<int, TerritoryTypeForAddress>();
            TerritoryISOCode = new Dictionary<int, string>();
        }
        public AddressConfigurationSiteSettingsPartViewModel(
            AddressConfigurationSiteSettingsPart part, bool detail = false) : this(){

            _contentManager = part.ContentItem.ContentManager;

            ShippingCountriesHierarchyId = part.ShippingCountriesHierarchyId;
            CountriesHierarchy = part.ShippingCountriesHierarchyId == 0
                    ? null
                    : _contentManager
                        .Get<TerritoryHierarchyPart>(part.ShippingCountriesHierarchyId);

            if (detail && CountriesHierarchy != null) {
                SelectedCountries = part.SelectedCountries;
                SelectedProvinces = part.SelectedProvinces;
                SelectedCities = part.SelectedCities;
                CountryCodes = part.CountryCodes;
                InitializeTerritories();
            }
        }

        private IContentManager _contentManager;

        #region base configuration
        public int ShippingCountriesHierarchyId { get; set; }
        public TerritoryHierarchyPart CountriesHierarchy { get; set; }
        public IEnumerable<TerritoryHierarchyPart> AllHierarchies { get; set; }

        public IEnumerable<SelectListItem> ListHierarchies() {
            var result = AllHierarchies
                .Select(thp => new SelectListItem {
                    Selected = ShippingCountriesHierarchyId == thp.Id,
                    Text = _contentManager.GetItemMetadata(thp).DisplayText,
                    Value = thp.Id.ToString()
                });

            return result;
        }
        #endregion

        #region details configuration

        public int[] SelectedCountries { get; set; }
        public int[] SelectedProvinces { get; set; }
        public int[] SelectedCities { get; set; }
        public CountryAlpha2[] CountryCodes { get; set; }

        public IEnumerable<AddressConfigurationTerritoryViewModel> TopLevel { get; set; }

        /// <summary>
        /// Dictionary to figure out what territories are countries/provinces/cities
        /// Key: Id of TerritoryInternalRecord
        /// Value: territory type (None/Country/Province/City)
        /// </summary>
        public IDictionary<int, TerritoryTypeForAddress> TerritoryTypeMap { get; set; }
        /// <summary>
        /// Dictionary for ISO codes for territories. We may be able to set codes for all
        /// territories, but the intent for now is to have it for countries only.
        /// Key: Id of TerritoryInternalRecord
        /// Value: Alpha-2 ISO 3166-1 code
        /// </summary>
        public IDictionary<int, string> TerritoryISOCode { get; set; }

        public void InitializeTerritories() {
            TopLevel = CountriesHierarchy.TopLevel
                // Create the top level and recursively create the whole hierarchy
                .Select(ci => {
                    var tp = ci.As<TerritoryPart>();
                    return tp != null
                        ? new AddressConfigurationTerritoryViewModel(
                            tp, SelectedCountries, SelectedProvinces, SelectedCities, CountryCodes)
                        : null;
                })
                // remove nulls (sanity check)
                .Where(tvm => tvm != null);
            // Initialize the dictionaries we'll use to edit the configuration
            TerritoryTypeMap = new Dictionary<int, TerritoryTypeForAddress>();
            foreach (var territory in CountriesHierarchy.Territories) {
                var part = territory.As<TerritoryPart>();
                if (part != null) { // sanity check
                    var tType = TerritoryTypeForAddress.None;
                    var internalId = part.Record.TerritoryInternalRecord.Id;
                    if (SelectedCountries.Contains(internalId)) {
                        tType = TerritoryTypeForAddress.Country;
                    } else if(SelectedProvinces.Contains(internalId)) {
                        tType = TerritoryTypeForAddress.Province;
                    } else if(SelectedCities.Contains(internalId)) {
                        tType = TerritoryTypeForAddress.City;
                    }
                    TerritoryTypeMap.Add(internalId, tType);
                    var iso = CountryCodes
                        // default is a struct with ISOCode = null
                        .FirstOrDefault(cc => cc.TerritoryId == internalId)
                        .ISOCode ?? string.Empty;
                    TerritoryISOCode.Add(internalId, iso);
                }
            }
        }

        public void ResetDetails() {
            // TODO: clear all detail configuration
        }
        #endregion

    }
}
