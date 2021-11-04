using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.Providers {
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