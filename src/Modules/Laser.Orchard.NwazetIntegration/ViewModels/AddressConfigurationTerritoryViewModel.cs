using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class AddressConfigurationTerritoryViewModel {

        protected AddressConfigurationTerritoryViewModel() {
            Children = new List<AddressConfigurationTerritoryViewModel>();
        }

        public AddressConfigurationTerritoryViewModel(
            TerritoryPart part, 
            IEnumerable<int> countries,
            IEnumerable<int> provinces,
            IEnumerable<int> cities,
            IEnumerable<CountryAlpha2> countryCodes) : this() {

            _contentManager = part.ContentItem.ContentManager;

            Territory = part;
            TerritoryId = part.Record.TerritoryInternalRecord.Id;

            IsCountry = countries.Contains(TerritoryId);
            IsProvince = provinces.Contains(TerritoryId);
            IsCity = cities.Contains(TerritoryId);

            CountryISO = countryCodes
                // if there is no value for the given ID, FirstOrDefault
                // returns a CountryAlpha2 struct with ISOCode = null. THis
                // happens because CountryAlpha2 is a struct rather than a class
                .FirstOrDefault(cc => cc.TerritoryId == TerritoryId)
                .ISOCode ?? string.Empty;

            DisplayText = _contentManager
                .GetItemMetadata(part)
                .DisplayText;
            if (string.IsNullOrWhiteSpace(DisplayText)) {
                DisplayText = part.Record.TerritoryInternalRecord.Name;
            }

            foreach (var ci in part.Children) {
                var tp = ci.As<TerritoryPart>();
                if (tp != null) {
                    // constructing this here means we are going depth first
                    var child = new AddressConfigurationTerritoryViewModel(
                        tp, countries, provinces, cities, countryCodes, this);
                    Children.Add(child);
                }
            }
        }

        public AddressConfigurationTerritoryViewModel(
            TerritoryPart part,
            IEnumerable<int> countries,
            IEnumerable<int> provinces,
            IEnumerable<int> cities,
            IEnumerable<CountryAlpha2> countryCodes,
            AddressConfigurationTerritoryViewModel parent) 
            : this(part, countries, provinces, cities, countryCodes) {
            Parent = parent;
        }


        private IContentManager _contentManager;

        public TerritoryPart Territory { get; set; }
        /// <summary>
        /// Id of the TerritoryInternalRecord
        /// </summary>
        public int TerritoryId { get; set; }
        public string DisplayText { get; set; }

        public AddressConfigurationTerritoryViewModel Parent { get; set; }
        // This will be the first level children
        public List<AddressConfigurationTerritoryViewModel> Children { get; set; }

        public bool IsCountry { get; set; }
        public bool IsProvince { get; set; }
        public bool IsCity { get; set; }

        /// <summary>
        /// ISO 3166-1 Alpha-2 code for a country.
        /// </summary>
        public string CountryISO { get; set; }

        public int ChildCountries =>
            // how many of this object's children are Countries?
            Children.Count(vm => vm.IsCountry)
            // how many of their children are Countries?
            + Children.Sum(vm => vm.ChildCountries);
        public int ChildProvinces =>
            // how many of this object's children are Provinces?
            Children.Count(vm => vm.IsProvince)
            // how many of their children are Provinces?
            + Children.Sum(vm => vm.ChildProvinces);
        public int ChildCities =>
            // how many of this object's children are Cities?
            Children.Count(vm => vm.IsCity)
            // how many of their children are Cities?
            + Children.Sum(vm => vm.ChildCities);
    }
}