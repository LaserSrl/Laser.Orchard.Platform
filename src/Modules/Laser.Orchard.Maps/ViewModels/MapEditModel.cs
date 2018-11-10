using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Maps.Models;

namespace Laser.Orchard.Maps.ViewModels {
    public class MapEditModel {
        public MapsProviders MapProvider { get; set; }
        public string MapTiles { get; set; }
        public int MaxZoom { get; set; }
        public MapPart Map { get; set; }
        public string GoogleApiKey { get; set; }
        public string DecimalSeparator { get; set; } // #GM 2015-09-15
    }
}