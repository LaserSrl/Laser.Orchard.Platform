using Laser.Orchard.NwazetIntegration.Models;
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
            IEnumerable<int> cities) : this() {

            _contentManager = part.ContentItem.ContentManager;

            Territory = part;
            TerritoryId = part.Record.TerritoryInternalRecord.Id;

            var adminTypePart = part.As<TerritoryAdministrativeTypePart>();
            if (adminTypePart != null) {
                AdministrativeType = adminTypePart.AdministrativeType;
            } else {
                AdministrativeType = TerritoryAdministrativeType.None;
            }

            IsCountry = AdministrativeType == TerritoryAdministrativeType.Country;
            IsProvince = AdministrativeType == TerritoryAdministrativeType.Province;
            IsCity = AdministrativeType == TerritoryAdministrativeType.City;

            DisplayText = _contentManager
                .GetItemMetadata(part)
                .DisplayText;
            if (string.IsNullOrWhiteSpace(DisplayText)) {
                DisplayText = part.Record.TerritoryInternalRecord.Name;
            }

            ChildrenCount = part.Record.Children.Count();
        }

        public AddressConfigurationTerritoryViewModel(
            TerritoryPart part,
            IEnumerable<int> countries,
            IEnumerable<int> provinces,
            IEnumerable<int> cities,
            AddressConfigurationTerritoryViewModel parent) 
            : this(part, countries, provinces, cities) {
            Parent = parent;
        }

        private IContentManager _contentManager;

        public TerritoryPart Territory { get; set; }
        /// <summary>
        /// Id of the TerritoryInternalRecord
        /// </summary>
        public int TerritoryId { get; set; }
        public string DisplayText { get; set; }

        public TerritoryAdministrativeType AdministrativeType { get; set; }

        public AddressConfigurationTerritoryViewModel Parent { get; set; }
        // This will be the first level children
        public List<AddressConfigurationTerritoryViewModel> Children { get; set; }

        public bool IsCountry { get; set; }
        public bool IsProvince { get; set; }
        public bool IsCity { get; set; }
        
        public int ChildrenCount { get; set; }

        public int AllChildrenCount =>
            // count of this object's direct children
            Children.Count()
            // and all their children
            + Children.Sum(vm => vm.AllChildrenCount);
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