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
        }
        public AddressConfigurationSiteSettingsPartViewModel(
            AddressConfigurationSiteSettingsPart part) : this(){

            _contentManager = part.ContentItem.ContentManager;
        }

        private IContentManager _contentManager;

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

        public string[] SelectedTerritories { get; set; }
    }
}
