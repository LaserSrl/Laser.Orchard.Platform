using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Providers;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.UI.Admin;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Services.Pos {
    public class CustomPosOrderFrontEndAdditionalInformationProvider : BaseOrderAdditionalInformationProvider,
        IOrderFrontEndAdditionalInformationProvider {

        private readonly IPaymentService _paymentService;
        private readonly IList<ICustomPosProvider> _customPosProviders;
        private readonly dynamic _shapeFactory;
        private readonly IWorkContextAccessor _workContextAccessor;

        public CustomPosOrderFrontEndAdditionalInformationProvider(IPaymentService paymentService,
            IList<ICustomPosProvider> customPosProviders,
            IShapeFactory shapeFactory,
            IWorkContextAccessor workContextAccessor) {
            _paymentService = paymentService;
            _customPosProviders = customPosProviders;
            _shapeFactory = shapeFactory;
            _workContextAccessor = workContextAccessor;
        }

        public override IEnumerable<dynamic> GetAdditionalOrderMetadataShapes(OrderPart orderPart) {
            // I need to avoid showing these shapes if I'm in the backoffice.
            if (!AdminFilter.IsApplied(_workContextAccessor.GetContext().HttpContext.Request.RequestContext)) {
                var transactionId = orderPart.Charge?.TransactionId;
                if (transactionId != null) {
                    PaymentRecord payment = _paymentService.GetPaymentByTransactionId(transactionId);
                    if (payment != null) {
                        var metaShapes = _customPosProviders
                            .Select(cpp => cpp.GetAdditionalFrontEndMetadataShapes(payment))
                            .ToList();
                        // metaShapes variable is a list of lists.
                        foreach (var l in metaShapes) {
                            foreach (var shape in l.ToList()) {
                                yield return shape;
                            }
                        }
                    }
                }
            }
        }
    }
}