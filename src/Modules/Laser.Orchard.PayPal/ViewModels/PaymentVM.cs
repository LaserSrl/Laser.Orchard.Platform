using Laser.Orchard.PaymentGateway.Models;

namespace Laser.Orchard.PayPal.ViewModels {
    public class PaymentVM {
        public PaymentRecord Record { get; set; }
        public string TenantBaseUrl { get; set; }
    }
}