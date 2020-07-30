using Laser.Orchard.NwazetIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class CheckoutSettingsPartViewModel {
        public CheckoutSettingsPartViewModel() { }
        public CheckoutSettingsPartViewModel(CheckoutSettingsPart part) {
            CheckoutRequiresAuthentication = part.CheckoutRequiresAuthentication;
            PhoneIsRequired = part.PhoneIsRequired;
        }

        public bool CheckoutRequiresAuthentication { get; set; }
        public bool PhoneIsRequired { get; set; }
    }
}