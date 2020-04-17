using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.NwazetIntegration.Models;
using Nwazet.Commerce.Models;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class AddressesVM {
        public string Submit { get; set; }
        public Address ShippingAddress { get; set; }
        public AddressEditViewModel ShippingAddressVM { get; set; }
        public Address BillingAddress { get; set; }
        public AddressEditViewModel BillingAddressVM { get; set; }
        public string Email { get; set; }
        public string PhonePrefix { get; set; }
        public string Phone { get; set; }
        public string SpecialInstructions { get; set; }
        public List<AddressRecord> ListAvailableShippingAddress { get; set; }
        public List<AddressRecord> ListAvailableBillingAddress { get; set; }
        public AddressesVM() {
            ListAvailableShippingAddress = new List<AddressRecord>();
            ListAvailableBillingAddress = new List<AddressRecord>();

        }
    }
}