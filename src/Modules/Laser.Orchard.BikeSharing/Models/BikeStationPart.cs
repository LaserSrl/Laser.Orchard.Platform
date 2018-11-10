using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.BikeSharing.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Laser.Orchard.BikeSharing.Models {
    public class BikeStationPart : ContentPart {
        internal LazyField<BikeStationInfos> _bikeStation = new LazyField<BikeStationInfos>();

        public string BikeStationUName {
            get { return this.Retrieve(x => x.BikeStationUName); }
            set { this.Store(x => x.BikeStationUName, value); }
        }

        public BikeStationInfos Info {
            get {
                return _bikeStation.Value;
            }
        }
    }
}