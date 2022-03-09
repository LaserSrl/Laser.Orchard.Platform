using Laser.Orchard.PaymentGateway.Models;
using Nwazet.Commerce.Services;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Services.Pos {
    public abstract class PosIntegrationServiceBase : IPosIntegrationService {
        private static readonly OrderStatus paymentSucceededState =
            new OrderStatus { StatusName = Constants.PaymentSucceeded, Priority = "2.1" };

        public virtual bool CheckPos(PaymentRecord payment) {
            return false;
        }

        public virtual OrderStatus GetPaymentSuccessOrderStatus() {
            return paymentSucceededState;
        }
    }
}