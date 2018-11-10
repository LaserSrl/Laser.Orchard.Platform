using Laser.Orchard.PaymentGateway.Models;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.ViewModels {
    public class PaymentListVM {
        public List<PaymentRecord> Records { get; set; }
        public dynamic Pager { get; set; }
    }
}