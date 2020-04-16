using Laser.Orchard.NwazetIntegration.Services;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class ShippingCountriesTerritoryPartDriver
        : ContentPartDriver<TerritoryPart> {

        private readonly IAddressConfigurationSettingsService _addressConfigurationSettingsService;

        public ShippingCountriesTerritoryPartDriver(
            IAddressConfigurationSettingsService addressConfigurationSettingsService) {

            _addressConfigurationSettingsService = addressConfigurationSettingsService;
        }

        protected override string Prefix {
            get { return "TerritoryPart"; }
        }

        protected override DriverResult Editor(
            TerritoryPart part, dynamic shapeHelper) {
            // we are going to add a shape to warn users that this territory
            // is selected as the one for the countries (if that is true)
            var settingsPart = _addressConfigurationSettingsService.ShippingCountriesHierarchy;
            if (settingsPart != null) {
                var hId = part.Hierarchy?.Id;
                var localizations = _addressConfigurationSettingsService.ShippingCountriesHierarchies;
                var hierarchyOk = hId.HasValue && localizations.Any()
                    && localizations.Select(thp => thp.Id).Contains(hId.Value);
                if (hierarchyOk) {
                    if (_addressConfigurationSettingsService.SelectedTerritoryRecords
                        .Any(tir => part.IsSameAs(tir))) {
                        // the territory is selected
                        return ContentShape("Parts_TerritoryPart_IsSelectedCountry",
                            () => shapeHelper.EditorTemplate(
                                TemplateName: "Parts/TerritoryPartIsSelectedCountry",
                                Prefix: Prefix
                            )
                        );
                    }
                }
            }

            return null;
        }
    }
}
