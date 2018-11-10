using Laser.Orchard.PaymentGestPay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGestPay.ViewModels {
    public class GestPaySettingsViewModel {
        public string GestPayShopLogin { get; set; }
        public bool UseTestEnvironment { get; set; }

        public GestPaySettingsViewModel() { }

        public GestPaySettingsViewModel(PaymentGestPaySettingsPart part) {
            GestPayShopLogin = part.GestPayShopLogin;
            UseTestEnvironment = part.UseTestEnvironment;
        }
    }
}