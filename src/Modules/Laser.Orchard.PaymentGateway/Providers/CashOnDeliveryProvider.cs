using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.Providers {
    public class CashOnDeliveryProvider : DefaultCustomPosProvider {

        public override string TechnicalName => "CashOnDelivery";

        public override string GetButtonShapeName() {
            return base.GetButtonShapeName();
        }

        public override string GetDisplayName() {
            return T("Cash on Delivery").Text;
        }

        public override string GetInfoShapeName() {
            return "CashOnDelivery";
        }
    }
}