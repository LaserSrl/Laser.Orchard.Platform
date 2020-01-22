using System.Collections.Generic;
using Nwazet.Commerce.Services;
using Orchard.Localization;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class PaymentStatusProvider : BaseOrderStatusProvider {

        public static readonly string[] states = {
           Constants.PaymentSucceeded
        };

        public override IEnumerable<string> States => states;

        public override Dictionary<string, LocalizedString> StatusLabels => 
            new Dictionary<string, LocalizedString> {
                { Constants.PaymentSucceeded, T("Payment Succeeded") }
            };
    }
}