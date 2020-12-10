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

        public bool PhoneIsRequired {
            get { return this.Retrieve(p => p.PhoneIsRequired); }
            set { this.Store(p => p.PhoneIsRequired, value); }
        }

        public bool ShippingIsRequired {
            get { return this.Retrieve(p => p.ShippingIsRequired); }
            set { this.Store(p => p.ShippingIsRequired, value); }
        }
    }
}