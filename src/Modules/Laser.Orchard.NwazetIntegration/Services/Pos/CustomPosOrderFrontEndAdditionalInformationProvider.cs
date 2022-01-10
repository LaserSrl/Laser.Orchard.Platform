using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Providers;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.DisplayManagement;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Services.Pos {
    public class CustomPosOrderFrontEndAdditionalInformationProvider : BaseOrderAdditionalInformationProvider,
        IOrderFrontEndAdditionalInformationProvider {

        private readonly IPaymentService _paymentService;
        private readonly IList<ICustomPosProvider> _customPosProviders;
        private readonly dynamic _shapeFactory;

        public CustomPosOrderFrontEndAdditionalInformationProvider(IPaymentService paymentService,
            IList<ICustomPosProvider> customPosProviders,
            IShapeFactory shapeFactory) {
            _paymentService = paymentService;
            _customPosProviders = customPosProviders;
            _shapeFactory = shapeFactory;
        }

        public override IEnumerable<dynamic> GetAdditionalOrderMetadataShapes(OrderPart orderPart) {
            var transactionId = orderPart.Charge?.TransactionId;
            if (transactionId != null) {
                PaymentRecord payment = _paymentService.GetPaymentByTransactionId(transactionId);
                if (payment != null) {
                    var metaShapes = _customPosProviders
                        .Select(cpp => cpp.GetAdditionalFrontEndMetadataShapes(payment));
                    foreach (var shape in metaShapes) {
                        yield return shape;
                    }
                }                
            }
            //yield return base.GetAdditionalOrderMetadataShapes(orderPart);
        }
    }
}