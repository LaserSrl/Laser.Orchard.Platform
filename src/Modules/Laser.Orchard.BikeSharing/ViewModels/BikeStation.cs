using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.BikeSharing.ViewModels {
    public class BikeStationInfos {
        public int AvailableBikes {
            get {
                return Terminals.Count(w => !String.IsNullOrWhiteSpace(w.BikeUid));
            }
        }
        public int Capacity {
            get {
                return Terminals.Count();
            }
        }
        public IList<Terminal> Terminals { get; set; }
    }

    public class Terminal {
        public string BikeUid { get; set; }
        public string BikeName { get; set; }
        public int BatteryLevel { get; set; }
        public int TerminalNumber { get; set; }
    }
}