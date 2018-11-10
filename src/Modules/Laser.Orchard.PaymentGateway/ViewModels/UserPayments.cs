using Laser.Orchard.PaymentGateway.Models;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.ViewModels {
    public class UserPayments {
        public List<PaymentRecord> Records { get; set; }
        public string UserName { get; set; }
    }
}