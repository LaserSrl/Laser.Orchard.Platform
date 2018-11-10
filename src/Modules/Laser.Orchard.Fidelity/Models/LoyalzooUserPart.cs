using Orchard.ContentManagement;

namespace Laser.Orchard.Fidelity.Models
{
    public class LoyalzooUserPart : ContentPart
    {
        public string LoyalzooUsername
        {
            get { return this.Retrieve(x => x.LoyalzooUsername); }
            set { this.Store(x => x.LoyalzooUsername, value); }
        }

        public string LoyalzooPassword
        {
            get { return this.Retrieve(x => x.LoyalzooPassword); }
            set { this.Store(x => x.LoyalzooPassword, value); }
        }

        public string CustomerSessionId
        {
            get { return this.Retrieve(x => x.CustomerSessionId); }
            set { this.Store(x => x.CustomerSessionId, value); }
        }
    }
}