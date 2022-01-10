using System.Web.Mvc;
using System.Web.Mvc.Html;
using Laser.Orchard.PaymentGateway.ViewModels;

namespace Laser.Orchard.PaymentGateway.Models {
    public class AdditionalPartial : AdditionalShapeBase {
        public override int Order { get => base.Order; set => base.Order = value; }
        public string ShapeFile { get; set; }

        public override dynamic DisplayShape(AdditionalShapeContext ctx) {
            return ctx.ShapeFactory.AdditionalShapeManager(ShapeName: ShapeFile,
                ShapeContext: ctx);
        }
    }
}