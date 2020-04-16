using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class ConfigurationRequestViewModel {
        public int CountryId { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        // default is false. Tells whether the information we are
        // asking for is for a billing address. If it's false, the 
        // address is considered to be for shipping.
        public bool IsBillingAddress { get; set; }
    }
}