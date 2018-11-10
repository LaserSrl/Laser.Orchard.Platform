using Orchard.ContentManagement;

namespace Laser.Orchard.PaymentGateway.Models {
    public class PaymentGatewaySiteSettingsPart : ContentPart {
        public int NonceMinutesDuration {
            get { return this.Retrieve(x => x.NonceMinutesDuration); }
            set { this.Store(x => x.NonceMinutesDuration, value); }
        }
    }
}