using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
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
            TerritoryTypeMap = new Dictionary<int, TerritoryAdministrativeType>();
            //TerritoryISOCode = new Dictionary<int, string>();
        }
        public AddressConfigurationSiteSettingsPartViewModel(
            AddressConfigurationSiteSettingsPart part,
            IAddressConfigurationSettingsService addressConfigurationSettingsService = null) : this() {

            _contentManager = part.ContentItem.ContentManager;

            ShippingCountriesHierarchyId = part.ShippingCountriesHierarchyId;
            CountriesHierarchy = part.ShippingCountriesHierarchyId == 0
                    ? null
                    : _contentManager
                        .Get<TerritoryHierarchyPart>(part.ShippingCountriesHierarchyId);

            if (addressConfigurationSettingsService != null && CountriesHierarchy != null) {
                SelectedCountries = addressConfigurationSettingsService.SelectedCountryIds;
                SelectedProvinces = addressConfigurationSettingsService.SelectedProvinceIds;
                SelectedCities = addressConfigurationSettingsService.SelectedCityIds;
                InitializeTerritories();
            }
        }

        private IContentManager _contentManager;
        private IAddressConfigurationSettingsService _addressConfigurationSettingsService;

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
        //public CountryAlpha2[] CountryCodes { get; set; }

        public IEnumerable<AddressConfigurationTerritoryViewModel> TopLevel { get; set; }

        /// <summary>
        /// Dictionary to figure out what territories are countries/provinces/cities
        /// Key: Id of TerritoryInternalRecord
        /// Value: territory type (None/Country/Province/City)
        /// </summary>
        public IDictionary<int, TerritoryAdministrativeType> TerritoryTypeMap { get; set; }
        ///// <summary>
        ///// Dictionary for ISO codes for territories. We may be able to set codes for all
        ///// territories, but the intent for now is to have it for countries only.
        ///// Key: Id of TerritoryInternalRecord
        ///// Value: Alpha-2 ISO 3166-1 code
        ///// </summary>
        //public IDictionary<int, string> TerritoryISOCode { get; set; }

        public void InitializeTerritories() {
            TopLevel = CountriesHierarchy.TopLevel
                // Create the top level and recursively create the whole hierarchy
                .Select(ci => {
                    var tp = ci.As<TerritoryPart>();
                    return tp != null
                        ? new AddressConfigurationTerritoryViewModel(
                            tp, SelectedCountries, SelectedProvinces, SelectedCities)
                        : null;
                })
                // remove nulls (sanity check)
                .Where(tvm => tvm != null);
            // Initialize the dictionaries we'll use to edit the configuration
            TerritoryTypeMap = new Dictionary<int, TerritoryAdministrativeType>();

            foreach (var territory in CountriesHierarchy.Record.Territories) {
                var tType = TerritoryAdministrativeType.None;
                var internalId = territory.TerritoryInternalRecord.Id;
                if (SelectedCountries.Contains(internalId)) {
                    tType = TerritoryAdministrativeType.Country;
                } else if (SelectedProvinces.Contains(internalId)) {
                    tType = TerritoryAdministrativeType.Province;
                } else if (SelectedCities.Contains(internalId)) {
                    tType = TerritoryAdministrativeType.City;
                }
                TerritoryTypeMap.Add(internalId, tType);
                //var iso = CountryCodes
                //    // default is a struct with ISOCode = null
                //    .FirstOrDefault(cc => cc.TerritoryId == internalId)
                //    .ISOCode ?? string.Empty;
                //TerritoryISOCode.Add(internalId, iso);
            }
        }

        public void ResetDetails() {
            // TODO: clear all detail configuration
        }
        #endregion

    }
}
