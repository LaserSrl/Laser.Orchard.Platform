using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class FacebookShopSiteSettingsViewModel {
        public string ApiBaseUrl { get; set; }

        public string DefaultJsonForProductUpdate { get; set; }

        public string UserName { get; set; }
        public string BusinessId { get; set; }
        public string CatalogId { get; set; }
    }
}