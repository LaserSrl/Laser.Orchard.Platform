using Orchard.ContentManagement;

namespace Laser.Orchard.FidelityGateway.Models
{
    public class FidelitySettingsPart : ContentPart
    {
        public string DeveloperKey
        {
            get { return this.Retrieve(x => x.DeveloperKey); }
            set { this.Store(x => x.DeveloperKey, value); }
        }

        public string ApiURL
        {
            get { return this.Retrieve(x => x.ApiURL); }
            set { this.Store(x => x.ApiURL, value); }
        }

        public string UserID
        {
            get { return this.Retrieve(x => x.UserID); }
            set { this.Store(x => x.UserID, value); }
        }

        public string Password
        {
            get { return this.Retrieve(x => x.Password); }
            set { this.Store(x => x.Password, value); }
        }

        public string AccountID
        {
            get { return this.Retrieve(x => x.AccountID); }
            set { this.Store(x => x.AccountID, value); }
        }

        public string DefaultCampaign
        {
            get { return this.Retrieve(x => x.DefaultCampaign); }
            set { this.Store(x => x.DefaultCampaign, value); }
        }
    }
}