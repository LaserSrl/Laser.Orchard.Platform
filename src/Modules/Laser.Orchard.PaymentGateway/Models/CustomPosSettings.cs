namespace Laser.Orchard.PaymentGateway.Models {
    public class CustomPosSettings {
        public string ProviderName { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string ShapeName { get; set; }
        public string OrderMessage { get; set; }
        public bool Delete { get; set; }
    }
}