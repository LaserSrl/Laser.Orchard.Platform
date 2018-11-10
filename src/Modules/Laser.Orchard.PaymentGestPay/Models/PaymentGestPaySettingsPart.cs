using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGestPay.Models {
    public class PaymentGestPaySettingsPart : ContentPart<PaymentGestPaySettingsPartRecord> {

        public string GestPayShopLogin {
            get { return Record.GestPayShopLogin; }
            set { Record.GestPayShopLogin = value; }
        }
        public bool UseTestEnvironment {
            get { return Record.UseTestEnvironment; }
            set { Record.UseTestEnvironment = value; }
        }
    }
}