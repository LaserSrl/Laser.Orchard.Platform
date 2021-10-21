using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.Models {
    public class CustomPosSettings {
        public string Name { get; set; }
        public int Order { get; set; }
        public string ShapeName { get; set; }
        public string OrderMessage { get; set; }
    }
}