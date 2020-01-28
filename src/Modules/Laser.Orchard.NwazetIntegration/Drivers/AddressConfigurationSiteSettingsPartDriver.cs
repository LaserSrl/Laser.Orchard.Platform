using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Newtonsoft.Json;
using Nwazet.Commerce.Controllers;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class AddressConfigurationSiteSettingsPartDriver
        : ContentPartDriver<AddressConfigurationSiteSettingsPart> {

        private readonly IContentManager _contentManager;

        public AddressConfigurationSiteSettingsPartDriver(
            IContentManager contentManager) {

            _contentManager = contentManager;
        }

        protected override string Prefix {
            get { return "AddressConfigurationSiteSettings"; }
        }

        protected override DriverResult Editor(AddressConfigurationSiteSettingsPart part, dynamic shapeHelper) {
            return ContentShape("SiteSettings_AddressConfiguration",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "SiteSettings/AddressConfiguration",
                    Model: CreateVM(part),
                    Prefix: Prefix
                    )
                ).OnGroup("ECommerceSiteSettings");
        }

        protected override DriverResult Editor(AddressConfigurationSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new AddressConfigurationSiteSettingsPartViewModel();
            if (updater is ECommerceSettingsAdminController
                && updater.TryUpdateModel(model, Prefix, null, null)) {
                if (part.ShippingCountriesHierarchyId != model.ShippingCountriesHierarchyId) {
                    // selected hierarchy changed, so we need to reset
                    // the selected territories
                    part.SerializedSelectedTerritories = "[]";
                } else {
                    // update selected territories
                    part.SerializedSelectedTerritories = JsonConvert.SerializeObject(model.SelectedTerritories);
                }
                // selected hierarchy
                part.ShippingCountriesHierarchyId = model.ShippingCountriesHierarchyId;
            }
            return Editor(part, shapeHelper);
        }

        private AddressConfigurationSiteSettingsPartViewModel CreateVM(
            AddressConfigurationSiteSettingsPart part) {
            return new AddressConfigurationSiteSettingsPartViewModel(part) {
                ShippingCountriesHierarchyId = part.ShippingCountriesHierarchyId,
                CountriesHierarchy = part.ShippingCountriesHierarchyId == 0
                    ? null
                    : _contentManager.Get<TerritoryHierarchyPart>(part.ShippingCountriesHierarchyId),
                AllHierarchies = _contentManager
                    .Query<TerritoryHierarchyPart>(VersionOptions.Published)
                    .List(),
                SelectedTerritories = part.SelectedTerritories
            };
        }
    }
}
