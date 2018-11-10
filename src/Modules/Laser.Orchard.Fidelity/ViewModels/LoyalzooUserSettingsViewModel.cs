using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Fidelity.ViewModels
{
    public class LoyalzooUserSettingsViewModel
    {
        public int PartId { get; set; }

        public string LoyalzooUsername { get; set; }

        public string LoyalzooPassword { get; set; }

        public string CustomerSessionId { get; set; }
    }
}