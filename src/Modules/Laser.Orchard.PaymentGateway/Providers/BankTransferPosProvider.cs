using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Linq;

namespace Laser.Orchard.PaymentGateway.Providers {
    [OrchardFeature("Laser.Orchard.CustomPaymentGateway")]
    public class BankTransferPosProvider : DefaultCustomPosProvider {
        public BankTransferPosProvider(IWorkContextAccessor workContextAccessor) : base(workContextAccessor) {
            
        }

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