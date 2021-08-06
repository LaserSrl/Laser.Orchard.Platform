using Laser.Orchard.Braintree.Models;
using Laser.Orchard.PaymentGateway.Models;

namespace Laser.Orchard.Braintree.Services {
    public interface IBraintreePosService : IPosService {
        PaymentRecord GetPaymentInfo(string paymentGUID);
        BraintreeSettings GetSettings();
    }
}
