using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Controllers;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class CheckoutSettingsPartDriver : ContentPartDriver<CheckoutSettingsPart> {

        protected override string Prefix {
            get { return "CheckoutSettingsPart"; }
        }

        protected override DriverResult Editor(CheckoutSettingsPart part, dynamic shapeHelper) {
            return ContentShape("SiteSettings_CheckoutSettings",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "SiteSettings/CheckoutSettings",
                    Model: new CheckoutSettingsPartViewModel(part),
                    Prefix: Prefix
                    )).OnGroup("ECommerceSiteSettings");
        }

        protected override DriverResult Editor(CheckoutSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            if (updater is ECommerceSettingsAdminController) {
                var vm = new CheckoutSettingsPartViewModel();
                if (updater.TryUpdateModel(vm, Prefix, null, null)) {
                    part.CheckoutRequiresAuthentication = vm.CheckoutRequiresAuthentication;
                    part.PhoneIsRequired = vm.PhoneIsRequired;
                    part.ShippingIsRequired = vm.ShippingIsRequired;
                }
            }
            return Editor(part, shapeHelper);
        }
    }
}