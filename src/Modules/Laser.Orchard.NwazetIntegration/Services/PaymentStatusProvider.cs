using System;
using System.Collections.Generic;
using Nwazet.Commerce.Services;
using Orchard.Localization;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class PaymentStatusProvider : BaseOrderStatusProvider {

        #region private 
        private const string orderNumber = "2.{0}";

        private static readonly OrderStatus paymentSucceededState =
            new OrderStatus { StatusName = Constants.PaymentSucceeded, Priority = "2.1" };

        private static readonly OrderStatus paymentFailedState =
            new OrderStatus { StatusName = Constants.PaymentFailed, Priority = "2.2" };
        #endregion

        public static readonly string[] states = {
           Constants.PaymentSucceeded, Constants.PaymentFailed
        };

        public override IEnumerable<string> States => states;

        public override Dictionary<OrderStatus, LocalizedString> StatusLabels =>
         new Dictionary<OrderStatus, LocalizedString> {
            { paymentSucceededState, T("Payment Succeeded") },
            { paymentFailedState, T("Payment Failed") }
         };
    }
}