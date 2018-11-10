using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentCartaSi.Models {
    public class PaymentCartaSiSettingsPartRecord : ContentPartRecord {
        public virtual string CartaSiShopAlias { get; set; }
        public virtual string CartaSiSecretKey { get; set; }
        public virtual bool UseTestEnvironment { get; set; }
    }
}