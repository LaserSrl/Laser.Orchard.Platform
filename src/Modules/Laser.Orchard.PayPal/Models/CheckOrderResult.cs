using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PayPal.Models {
    public class CheckOrderResult {
        public bool Success { get; set; }
        public string MessageError { get; set; }
        public string Info { get; set; }
    }
}