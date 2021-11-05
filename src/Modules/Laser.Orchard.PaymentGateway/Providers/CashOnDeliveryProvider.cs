using Orchard.Environment.Extensions;

namespace Laser.Orchard.PaymentGateway.Providers {
    [OrchardFeature("Laser.Orchard.CustomPaymentGateway")]
    public class CashOnDeliveryProvider : DefaultCustomPosProvider {

        public override string TechnicalName => "CashOnDelivery";

        public override string GetButtonShapeName() {
            return "CashOnDelivery";
        }

        public override string GetDisplayName() {
            return T("Cash on Delivery").Text;
        }

        public override string GetInfoShapeName() {
            return "CashOnDelivery";
        }
    }
}