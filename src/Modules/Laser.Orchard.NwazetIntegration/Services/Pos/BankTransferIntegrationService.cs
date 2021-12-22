using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Providers;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.Core.Common.Fields;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Services.Pos {
    public class BankTransferIntegrationService : PosIntegrationServiceBase {
        private static readonly OrderStatus paymentPendingState =
            new OrderStatus { StatusName = Constants.PaymentPending, Priority = "2.3" };

        private static readonly string providerTechnicalName = "BankTransfer";

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IList<ICustomPosProvider> _customPosProviders;

        public BankTransferIntegrationService(IWorkContextAccessor workContextAccessor,
            IList<ICustomPosProvider> customPosProviders) {
            _workContextAccessor = workContextAccessor;
            _customPosProviders = customPosProviders;
        }

        public override bool CheckPos(PaymentRecord payment) {
            if (_customPosProviders != null) {
                var posProvider = _customPosProviders
                    .FirstOrDefault(cpp => cpp.TechnicalName.Equals(providerTechnicalName, StringComparison.InvariantCultureIgnoreCase));
                if (posProvider != null) {
                    return !string.IsNullOrWhiteSpace(posProvider.GetPosName(payment));
                }
            }
            return base.CheckPos(payment);
        }

        public override OrderStatus GetPaymentSuccessOrderStatus() {
            return paymentPendingState;
        }
    }
}