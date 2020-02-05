using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Maps.Models {
    [OrchardFeature("Laser.Orchard.Maps.OpenRoute")]
    public class OpenRouteSiteSettingsPart : ContentPart {
        public string ApiKey {
            get { return this.Retrieve(ak => ak.ApiKey); }
            set { this.Store(ak => ak.ApiKey, value); }
        }
    }
}