using Laser.Orchard.NwazetIntegration.Services;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class ShippingCountriesHierarchyPartDriver
        : ContentPartDriver<TerritoryHierarchyPart> {

        private readonly IAddressConfigurationSettingsService _addressConfigurationSettingsService;

        public ShippingCountriesHierarchyPartDriver(
            IAddressConfigurationSettingsService addressConfigurationSettingsService) {

            _addressConfigurationSettingsService = addressConfigurationSettingsService;
        }

        protected override string Prefix {
            get { return "TerritoryHierarchyPart"; }
        }

        protected override DriverResult Editor(
            TerritoryHierarchyPart part, dynamic shapeHelper) {
            // we are going to add a shape to warn users that this hierarchy
            // is selected as the one for the countries (if that is true)
            return ContentShape("Parts_TerritoryHierarchy_IsCountriesHierarchy",
                () => {
                    var settingsPart = _addressConfigurationSettingsService.ShippingCountriesHierarchy;
                    if (settingsPart != null) {
                        if (settingsPart.Id == part.Id) {
                            return shapeHelper.EditorTemplate(
                                TemplateName: "Parts/TerritoryHierarchyIsCountriesHierarchy",
                                Prefix: Prefix);
                        } else if (_addressConfigurationSettingsService.ShippingCountriesHierarchies
                            .Select(thp => thp.Id)
                            .Contains(part.Id)) {
                            return shapeHelper.EditorTemplate(
                                TemplateName: "Parts/TerritoryHierarchyIsCountriesHierarchyLocalization",
                                Prefix: Prefix);
                        }
                    }
                    return null;
                });
        }
    }
}
