using Orchard.Environment.Extensions;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopSiteSettingsViewModel {
        public string ApiBaseUrl { get; set; }

        public string DefaultJsonForProductUpdate { get; set; }

        public string BusinessId { get; set; }
        public string CatalogId { get; set; }
        //public string AppId { get; set; }
        //public string AppSecret { get; set; }
        public string AccessToken { get; set; }
        public bool SynchronizeProducts { get; set; }
    }
}