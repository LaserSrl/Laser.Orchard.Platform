using Laser.Orchard.PaymentGateway.Models;
using Nwazet.Commerce.Services;
using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.NwazetIntegration.Services.Pos {
    public interface IPosIntegrationService : IDependency {
        /// <summary>
        /// Checks if this is the right service to interrogate based on the payment record.
        /// For custom pos, this is based on the provider technical name.
        /// </summary>
        /// <returns></returns>
        bool CheckPos(PaymentRecord payment);
        /// <summary>
        /// Returns the order status for a succeeded payment.
        /// </summary>
        /// <returns></returns>
        OrderStatus GetPaymentSuccessOrderStatus();
    }
}
