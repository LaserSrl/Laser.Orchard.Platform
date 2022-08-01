using Laser.Orchard.NwazetIntegration.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointOrderViewModel {
        public PickupPointOrderPart Part { get; set; }
        public bool AddressHasChanged { get; set; }
    }
}