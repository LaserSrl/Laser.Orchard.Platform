using Nwazet.Commerce.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services.CheckoutShippingAddressProviders {
    public class ExtendedShippingOptionComputeContext : ShippingOptionComputeContext {

        // Explicitly carry territory Ids to get rid of string hacks
        // and make this easier to access and parse. Classes that wish to use this
        // should probably be checking that the context is an implementation of this
        // class.
        public int CountryId { get; set; }
        public int ProvinceId { get; set; } // Province / State
        public int CityId { get; set; }

        // These properties can be used to dynamically rebuild the information from the
        // shipping address providers during the checkout process.
        public string ShippingAddressProviderId { get; set; }
        public object ShippingAddressProviderViewModel { get; set; }
    }
}