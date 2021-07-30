using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;

namespace Laser.Orchard.NwazetIntegration.Services.FacebookShop {
    public class FacebookShopServiceContext {
        public string ApiBaseUrl { get; set; }

        public string DefaultJsonForProductUpdate { get; set; }

        public string BusinessId { get; set; }
        public string CatalogId { get; set; }
        public string AppId { get; set; }
        public string AppSecret { get; set; }

        public static FacebookShopServiceContext From(FacebookShopSiteSettingsViewModel viewModel) {
            return new FacebookShopServiceContext() {
                ApiBaseUrl = viewModel.ApiBaseUrl,
                DefaultJsonForProductUpdate = viewModel.DefaultJsonForProductUpdate,
                BusinessId = viewModel.BusinessId,
                CatalogId = viewModel.CatalogId,
                AppId = viewModel.AppId,
                AppSecret = viewModel.AppSecret
            };
        }

        public static FacebookShopServiceContext From(FacebookShopSiteSettingsPart part) {
            return new FacebookShopServiceContext() {
                ApiBaseUrl = part.ApiBaseUrl,
                DefaultJsonForProductUpdate = part.DefaultJsonForProductUpdate,
                BusinessId = part.BusinessId,
                CatalogId = part.CatalogId,
                AppId = part.AppId,
                AppSecret = part.AppSecret
            };
        }
    }
}