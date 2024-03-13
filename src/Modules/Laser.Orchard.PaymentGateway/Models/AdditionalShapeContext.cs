using Laser.Orchard.PaymentGateway.ViewModels;

namespace Laser.Orchard.PaymentGateway.Models {
    public class AdditionalShapeContext {
        public PaymentVM PaymentViewModel { get; set; }
        public dynamic ShapeFactory { get; set; }
    }
}