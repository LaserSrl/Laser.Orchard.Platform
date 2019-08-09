using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.PerItemCache")]
    public class PerItemCachePartSettings {
        public string PerItemKeyParam { get; set; }
    }
}