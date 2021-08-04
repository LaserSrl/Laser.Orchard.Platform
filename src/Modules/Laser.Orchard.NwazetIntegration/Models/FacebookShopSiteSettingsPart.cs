using Laser.Orchard.NwazetIntegration.ViewModels;
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

        public string BusinessId {
            get { return this.Retrieve(x => x.BusinessId); }
            set { this.Store(x => x.BusinessId, value); }
        }

        public string CatalogId {
            get { return this.Retrieve(x => x.CatalogId); }
            set { this.Store(x => x.CatalogId, value); }
        }

        //public string AppId {
        //    get { return this.Retrieve(x => x.AppId); }
        //    set { this.Store(x => x.AppId, value); }
        //}

        //public string AppSecret {
        //    get { return this.Retrieve(x => x.AppSecret); }
        //    set { this.Store(x => x.AppSecret, value); }
        //}

        public string AccessToken {
            get { return this.Retrieve(x => x.AccessToken); }
            set { this.Store(x => x.AccessToken, value); }
        }

        public void Save(FacebookShopSiteSettingsViewModel viewModel) {
            ApiBaseUrl = viewModel.ApiBaseUrl;
            DefaultJsonForProductUpdate = viewModel.DefaultJsonForProductUpdate;
            BusinessId = viewModel.BusinessId;
            CatalogId = viewModel.CatalogId;
            //AppId = viewModel.AppId;
            //AppSecret = viewModel.AppSecret;
            AccessToken = viewModel.AccessToken;
        }
    }
}