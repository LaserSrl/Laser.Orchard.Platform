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

        public IEnumerable<AddressConfigurationTerritoryViewModel> TopLevel { get; set; }

        /// <summary>
        /// Dictionary to figure out what territories are countries/provinces/cities
        /// Key: Id of TerritoryInternalRecord
        /// Value: territory type (None/Country/Province/City)
        /// </summary>
        public IDictionary<int, TerritoryTypeForAddress> TerritoryTypeMap { get; set; }

        public void InitializeTerritories() {
            TopLevel = CountriesHierarchy.TopLevel
                .Select(ci => {
                    var tp = ci.As<TerritoryPart>();
                    return tp != null
                        ? new AddressConfigurationTerritoryViewModel(
                            tp, SelectedCountries, SelectedProvinces, SelectedCities)
                        : null;
                })
                .Where(tvm => tvm != null);

            TerritoryTypeMap = new Dictionary<int, TerritoryTypeForAddress>();
            foreach (var territory in CountriesHierarchy.Territories) {
                var part = territory.As<TerritoryPart>();
                if (part != null) {
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
                }
            }
        }

        public void ResetDetails() {
            // TODO: clear all detail configuration
        }
        #endregion

    }
}
