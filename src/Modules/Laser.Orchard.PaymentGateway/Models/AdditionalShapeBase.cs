using Laser.Orchard.PaymentGateway.ViewModels;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Models {
    public abstract class AdditionalShapeBase {
        public virtual int Order { get; set; }
        public virtual dynamic DisplayShape(AdditionalShapeContext ctx) {
            return null;
        }
    }
}