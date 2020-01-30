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
            TerritoryPart part) : this() {

            Territory = part;

            _contentManager = part.ContentItem.ContentManager;

            foreach (var ci in part.Children) {
                var tp = ci.As<TerritoryPart>();
                if (tp != null) {
                    // constructing this here means we are going depth first
                    var child = new AddressConfigurationTerritoryViewModel(tp, this);
                    Children.Add(child);
                }
            }
        }

        public AddressConfigurationTerritoryViewModel(
            TerritoryPart part, 
            AddressConfigurationTerritoryViewModel parent) : this(part) {
            Parent = parent;
        }


        private IContentManager _contentManager;

        public TerritoryPart Territory { get; set; }

        public AddressConfigurationTerritoryViewModel Parent { get; set; }
        // This will be the first level children
        public List<AddressConfigurationTerritoryViewModel> Children { get; set; }

        public bool IsCountry { get; set; }
        public bool IsProvince { get; set; }
        public bool IsCity { get; set; }

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