using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Laser.Orchard.PaymentGateway.Providers {
    [OrchardFeature("Laser.Orchard.CustomPaymentGateway")]
    public class CustomPosShapes : IShapeTableProvider {
        private readonly IList<ICustomPosProvider> _customPosProviders;

        public CustomPosShapes(IList<ICustomPosProvider> customPosProviders) {
            _customPosProviders = customPosProviders;
        }

        public void Discover(ShapeTableBuilder builder) {
            foreach (var cpp in _customPosProviders) {
                builder.Describe(cpp.TechnicalName)
                    .OnDisplaying(displaying => {
                        string posName = displaying.Shape.ShapeContext.PaymentViewModel.PosName;
                        // Normalize pos name to transform it to a valid shape name
                        // TODO: remove special characters, removing invalid characters
                        posName = posName.Trim()
                            .Replace(" ", "")
                            .Replace(".", "_")
                            .Replace("-", "__");

                        displaying.ShapeMetadata.Alternates.Add(cpp.TechnicalName + "__" + posName);
                    });
            }
        }
    }
}