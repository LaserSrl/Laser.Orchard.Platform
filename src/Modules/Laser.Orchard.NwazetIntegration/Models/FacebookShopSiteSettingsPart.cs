using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.NwazetIntegration.Models {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopSiteSettingsPart : ContentPart {
        public string ApiBaseUrl {
            get { return this.Retrieve(x => x.ApiBaseUrl); }
            set { this.Store(x => x.ApiBaseUrl, value); }
        }

        public string DefaultJsonForProductUpdate {
            get { return this.Retrieve(x => x.DefaultJsonForProductUpdate); }
            set { this.Store(x => x.DefaultJsonForProductUpdate, value); }
        }

        public string UserName {
            get { return this.Retrieve(x => x.UserName); }
            set { this.Store(x => x.UserName, value); }
        }

        public string BusinessId {
            get { return this.Retrieve(x => x.BusinessId); }
            set { this.Store(x => x.BusinessId, value); }
        }

        public string CatalogId {
            get { return this.Retrieve(x => x.CatalogId); }
            set { this.Store(x => x.CatalogId, value); }
        }
    }
}