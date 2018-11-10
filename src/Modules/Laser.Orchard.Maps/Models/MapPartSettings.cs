using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Maps.Models {
    public class MapPartSettings {
        public bool Required { get; set; }
        public bool HideMapSource { get; set; }
        public string HintText { get; set; }
    }
}