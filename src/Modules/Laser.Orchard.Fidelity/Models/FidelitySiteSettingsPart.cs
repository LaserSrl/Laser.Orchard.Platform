using Orchard.ContentManagement;

namespace Laser.Orchard.Fidelity.Models
{
    public class FidelitySiteSettingsPart : ContentPart
    {
        public string DeveloperKey
        {
            get { return this.Retrieve(x => x.DeveloperKey);  }
            set { this.Store(x => x.DeveloperKey, value);  }
        }

        public string ApiURL
        {
            get { return this.Retrieve(x => x.ApiURL); }
            set { this.Store(x => x.ApiURL, value); }
        }

        public string MerchantUsername
        {
            get { return this.Retrieve(x => x.MerchantUsername); }
            set { this.Store(x => x.MerchantUsername, value); }
        }

        public string MerchantPwd
        {
            get { return this.Retrieve(x => x.MerchantPwd); }
            set { this.Store(x => x.MerchantPwd, value); }
        }

        public string MerchantSessionId
        {
            get { return this.Retrieve(x => x.MerchantSessionId); }
            set { this.Store(x => x.MerchantSessionId, value); }
        }

        public string PlaceId
        {
            get { return this.Retrieve(x => x.PlaceId); }
            set { this.Store(x => x.PlaceId, value); }
        }

        public LoyalzooRegistrationEnum RegisterOnLogin
        {
            get { return this.Retrieve(x => x.RegisterOnLogin, LoyalzooRegistrationEnum.Never); }
            set { this.Store(x => x.RegisterOnLogin, value); }
        }
    }
}