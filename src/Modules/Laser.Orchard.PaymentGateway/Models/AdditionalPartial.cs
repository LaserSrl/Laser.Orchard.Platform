using Orchard.DisplayManagement;

namespace Laser.Orchard.PaymentGateway.Models {
    public class AdditionalPartial : AdditionalShapeBase {
        public override int Order { get => base.Order; set => base.Order = value; }
        public string ShapeFile { get; set; }

        public override dynamic DisplayShape(AdditionalShapeContext ctx) {
            return ctx.ShapeFactory.Create(ShapeFile, Arguments.From(new {
                ShapeContext = ctx
            }));
        }
    }
}