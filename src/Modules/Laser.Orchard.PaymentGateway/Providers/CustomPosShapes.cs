using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.IO;

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
                       posName = posName.Trim()
                            .Replace(" ", "")
                            .Replace(".", "_")
                            .Replace("-", "__");

                        // Remove invalid characters from the resulting string
                        var invalidChars = Path.GetInvalidFileNameChars();
                        foreach(var c in invalidChars) {
                            posName = posName.Replace(c.ToString(), "");
                        }

                        displaying.ShapeMetadata.Alternates.Add(cpp.TechnicalName + "__" + posName);
                    });
            }
        }
    }
}