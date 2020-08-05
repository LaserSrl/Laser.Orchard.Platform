using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.NwazetIntegration.Models;
using Newtonsoft.Json;
using Nwazet.Commerce.Models;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class AddressesVM {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Submit { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Address ShippingAddress { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AddressEditViewModel ShippingAddressVM { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Address BillingAddress { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AddressEditViewModel BillingAddressVM { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PhonePrefix { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Phone { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SpecialInstructions { get; set; }
        [JsonIgnore]
        public List<AddressRecord> ListAvailableShippingAddress { get; set; }
        [JsonIgnore]
        public List<AddressRecord> ListAvailableBillingAddress { get; set; }
        public AddressesVM() {
            ListAvailableShippingAddress = new List<AddressRecord>();
            ListAvailableBillingAddress = new List<AddressRecord>();

        }
    }
}