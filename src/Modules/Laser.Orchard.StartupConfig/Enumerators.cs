using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig {
    public enum InspectionType {
        Device, DeviceBrand
    }

    public enum DevicesBrands {
        Apple, Google, Windows, Blackberry, Unknown
    }

    public enum OrderType { 
        Default, PublishedUtc, Title
    }
}