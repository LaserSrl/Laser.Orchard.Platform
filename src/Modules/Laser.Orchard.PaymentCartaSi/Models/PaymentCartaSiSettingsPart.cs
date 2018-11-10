using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentCartaSi.Models {
    public class PaymentCartaSiSettingsPart : ContentPart<PaymentCartaSiSettingsPartRecord> {

        public string CartaSiShopAlias {
            get { return Record.CartaSiShopAlias; }
            set { Record.CartaSiShopAlias = value; }
        }
        public string CartaSiSecretKey {
            get { return Record.CartaSiSecretKey; }
            set { Record.CartaSiSecretKey = value; }
        }
        public bool UseTestEnvironment {
            get { return Record.UseTestEnvironment; }
            set { Record.UseTestEnvironment = value; }
        }
    }
}