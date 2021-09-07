using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.NwazetIntegration.Services.FacebookShop {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopServiceContext {
        public string ApiBaseUrl { get; set; }

        public string DefaultJsonForProductUpdate { get; set; }

        public string BusinessId { get; set; }
        public string CatalogId { get; set; }
        //public string AppId { get; set; }
        //public string AppSecret { get; set; }
        public string AccessToken { get; set; }

        public static FacebookShopServiceContext From(FacebookShopSiteSettingsViewModel viewModel) {
            return new FacebookShopServiceContext() {
                ApiBaseUrl = viewModel.ApiBaseUrl,
                DefaultJsonForProductUpdate = viewModel.DefaultJsonForProductUpdate,
                BusinessId = viewModel.BusinessId,
                CatalogId = viewModel.CatalogId,
                //AppId = viewModel.AppId,
                //AppSecret = viewModel.AppSecret,
                AccessToken = viewModel.AccessToken
            };
        }

        public static FacebookShopServiceContext From(FacebookShopSiteSettingsPart part) {
            return new FacebookShopServiceContext() {
                ApiBaseUrl = part.ApiBaseUrl,
                DefaultJsonForProductUpdate = part.DefaultJsonForProductUpdate,
                BusinessId = part.BusinessId,
                CatalogId = part.CatalogId,
                //AppId = part.AppId,
                //AppSecret = part.AppSecret,
                AccessToken = part.AccessToken
            };
        }
    }
}