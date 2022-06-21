using Laser.Orchard.NwazetIntegration.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointPartEditViewModel {

        public PickupPointPartEditViewModel() { }
        public PickupPointPart PickupPointPart { get; set; }
        public PickupPointAddressEditViewModel AddressVM { get; set; }
    }
}