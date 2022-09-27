using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class AdditionalIndexShippingAddressViewModel {
        public string UniqueProviderId { get; set; }
        public string TabTitle { get; set; }
        public string TabId { get; set; }
        public string NavId { get; set; }
        public dynamic TabShape { get; set; }
    }
}