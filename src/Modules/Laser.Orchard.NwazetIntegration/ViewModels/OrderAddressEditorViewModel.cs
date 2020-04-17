using Laser.Orchard.NwazetIntegration.Models;
using Nwazet.Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class OrderAddressEditorViewModel {

        public OrderAddressEditorViewModel() { }
        public OrderPart OrderPart { get; set; }
        public AddressOrderPart AddressOrderPart { get; set; }

        public AddressEditViewModel ShippingAddressVM { get; set; }
        public AddressEditViewModel BillingAddressVM { get; set; }
    }
}