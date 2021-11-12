using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ShortCodes {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class Descriptor {
        public string Signature { get; set; }
    }
}