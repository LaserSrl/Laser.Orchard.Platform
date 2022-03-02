using Orchard.Environment.Extensions;

namespace Laser.Orchard.PaymentGateway.Models {
    [OrchardFeature("Laser.Orchard.CustomPaymentGateway")]
    public class CustomPosSettings {
        public string ProviderName { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public bool Delete { get; set; }
    }
}