using Orchard.Localization;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Services.CheckoutConditions {
    public class CheckoutConditionValidationContext {
        public CheckoutConditionValidationContext() {
            UserMayCheckout = true;
        }

        // Inputs for the validation
        // Activities manipulating this context should get further
        // services they may need themselves, to avoid creating unnecessary
        // dependencies.
        public IUser User { get; set; }

        // Results of the validation
        // Activities manipulating this context should set these values.
        public bool UserMayCheckout { get; set; }
        public ActionResult Redirect { get; set; }
        public LocalizedString Message { get; set; }
    }
}