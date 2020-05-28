using Orchard.ContentManagement;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class CheckoutSettingsPart : ContentPart {
        /// <summary>
        /// When this is true, checkout will only be possible for authenticated
        /// users, taking precedence over any per-product setting.
        /// </summary>
        public bool CheckoutRequiresAuthentication {
            get { return this.Retrieve(p => p.CheckoutRequiresAuthentication); }
            set { this.Store(p => p.CheckoutRequiresAuthentication, value); }
        }
    }
}