using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGestPay.Models {
    public class PaymentGestPaySettingsPartRecord : ContentPartRecord {
        public virtual string GestPayShopLogin { get; set; }
        public virtual bool UseTestEnvironment { get; set; }
    }
}