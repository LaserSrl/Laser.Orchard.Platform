using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ShortCodes.Settings.Models {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class ShortCodesSettings {
        public string EnabledShortCodes { get; set; }
    }
}