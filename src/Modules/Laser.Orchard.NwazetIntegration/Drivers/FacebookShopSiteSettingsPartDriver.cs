using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopSiteSettingsPartDriver : ContentPartDriver<FacebookShopSiteSettingsPart> {
        private readonly IAuthorizer _authorizer;
     
        protected override string Prefix => "FacebookShopSiteSettingsPart";

        public FacebookShopSiteSettingsPartDriver(IAuthorizer authorizer) {
            _authorizer = authorizer;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(FacebookShopSiteSettingsPart part, dynamic shapeHelper) {
            return shapeHelper.EditorTemplate(
                TemplateName: "Parts/FacebookShopSiteSettings.cshtml",
                Model: CreateVM(part),
                Prefix: Prefix).OnGroup("FacebookShop");
        }

        protected override DriverResult Editor(FacebookShopSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = CreateVM(part);
            if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                if (Validate(viewModel, part, updater)) {

                } else {

                }
            }

            return Editor(part, shapeHelper);
        }

        private FacebookShopSiteSettingsViewModel CreateVM(FacebookShopSiteSettingsPart part) {
            return new FacebookShopSiteSettingsViewModel() {
                ApiBaseUrl = part.ApiBaseUrl,
                DefaultJsonForProductUpdate = part.DefaultJsonForProductUpdate,
                UserName = part.UserName,
                BusinessId = part.BusinessId,
                CatalogId = part.CatalogId
            };
        }

        private bool Validate(FacebookShopSiteSettingsViewModel viewModel, FacebookShopSiteSettingsPart part, IUpdateModel updater) {
            return true;
        }
    }
}