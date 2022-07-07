using Laser.Orchard.NwazetIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class PickupPointsCheckoutViewModel {
        // TODO: this class should be serializable with Newtonsoft.Json
        public int SelectedPickupPointId { get; set; }
        public PickupPointPart PickupPointPart { get; set; }
    }
}