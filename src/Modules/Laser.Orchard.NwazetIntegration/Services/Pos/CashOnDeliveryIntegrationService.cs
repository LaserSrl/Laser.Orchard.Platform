using Laser.Orchard.PaymentGateway.Models;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Services.Pos {
    public class CashOnDeliveryIntegrationService : PosIntegrationServiceBase {
        private static readonly OrderStatus paymentPendingState =
            new OrderStatus { StatusName = Constants.PaymentPending, Priority = "2.3" };

        private static readonly string providerTechnicalName = "CashOnDelivery";

        private readonly IWorkContextAccessor _workContextAccessor;

        public CashOnDeliveryIntegrationService(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public override bool CheckPos(PaymentRecord payment) {
            if (payment.PosName.StartsWith("CustomPos_")) {
                var customPosName = payment.PosName.Substring("CustomPos_".Length);

                var customPosSiteSettings = _workContextAccessor.GetContext().CurrentSite.As<CustomPosSiteSettingsPart>();
                var currentCustomPos = customPosSiteSettings.CustomPos
                    .FirstOrDefault(cps => cps.Name.Equals(customPosName)
                        && cps.ProviderName.Equals(providerTechnicalName, StringComparison.InvariantCultureIgnoreCase));

                return (currentCustomPos != null);
            }

            return base.CheckPos(payment);
        }

        public override OrderStatus GetPaymentSuccessOrderStatus() {
            return paymentPendingState;
        }
    }
}