using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.Models {
    public class PayButtonPartSettings {
        public string AmountField { get; set; }
        public string CurrencyField { get; set; }
        public string DefaultCurrency { get; set; }
    }
}