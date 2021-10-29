using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.Providers {
    public class BankTransferPosProvider : DefaultCustomPosProvider {

        public override string TechnicalName => base.TechnicalName;

        public override string GetButtonShapeName() {
            return base.GetButtonShapeName();
        }

        public override string GetDisplayName() {
            return T("Bank Transfer").Text;
        }

        public override string GetInfoShapeName() {
            return base.GetInfoShapeName();
        }
    }
}