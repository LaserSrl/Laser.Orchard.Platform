using Laser.Orchard.PaymentGateway.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Braintree.ViewModels {
    public class PaymentVM {
        public PaymentRecord Record { get; set; }
        public string TenantBaseUrl { get; set; }
    }
}