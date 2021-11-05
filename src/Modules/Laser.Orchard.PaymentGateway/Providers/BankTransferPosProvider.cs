using Orchard.Environment.Extensions;

namespace Laser.Orchard.PaymentGateway.Providers {
    [OrchardFeature("Laser.Orchard.CustomPaymentGateway")]
    public class BankTransferPosProvider : DefaultCustomPosProvider {

        public override string TechnicalName => "BankTransfer";

        public override string GetButtonShapeName() {
            return "BankTransfer";
        }

        public override string GetDisplayName() {
            return T("Bank Transfer").Text;
        }

        public override string GetInfoShapeName() {
            return "BankTransfer";
        }
    }
}