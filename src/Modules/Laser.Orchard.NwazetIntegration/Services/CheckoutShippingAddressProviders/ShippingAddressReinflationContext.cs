using Laser.Orchard.NwazetIntegration.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services.CheckoutShippingAddressProviders {
    public class ShippingAddressReinflationContext {
        public CheckoutViewModel TargetCheckoutViewModel { get; set; }
        public CheckoutViewModel SourceCheckoutViewModel { get; set; }
    }
}