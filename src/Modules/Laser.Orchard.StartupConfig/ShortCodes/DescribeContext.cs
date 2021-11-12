using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ShortCodes {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class DescribeContext {
        public string Name { get; set; }
        public string Signature { get; set; }
    }
}