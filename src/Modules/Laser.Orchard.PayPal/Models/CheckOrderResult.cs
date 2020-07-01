using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PayPal.Models {
    public class CheckOrderResult {
        public string Status { get; set; }
        public string OrderIdReturned { get; set; }
        public bool Error { get; set; }
    }
}