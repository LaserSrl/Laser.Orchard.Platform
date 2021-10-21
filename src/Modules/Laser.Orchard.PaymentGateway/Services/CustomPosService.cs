using System;
using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using Orchard.Data;

namespace Laser.Orchard.PaymentGateway.Services {
    public class CustomPosService : PosServiceBase {
        public CustomPosService(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler) : base(orchardServices, repository, paymentEventHandler) {
        }

        public override Type GetPosActionControllerType() {
            return typeof(object);
        }

        public override string GetPosActionName() {
            return "Index";
        }

        public override string GetPosActionUrl(int paymentId) {
            return string.Empty;
        }

        public override string GetPosActionUrl(string paymentGuid) {
            return string.Empty;
        }

        public override string GetPosName() {
            return "Custom Pos";
        }

        public override string GetPosUrl(int paymentId) {
            return string.Empty;
        }

        public override string GetSettingsControllerName() {
            return "CustomPosAdmin";
        }
    }
}