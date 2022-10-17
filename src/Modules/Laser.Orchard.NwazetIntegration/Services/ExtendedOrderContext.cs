using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class ExtendedOrderContext : OrderContext {
        public CheckoutViewModel CheckoutViewModel { get; set; }
    }
}