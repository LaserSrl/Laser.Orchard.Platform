using Orchard.Environment.Extensions;
using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile {
    [OrchardFeature("Laser.Orchard.SmsGateway")]
    public class ResourceManifest : IResourceManifestProvider {

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            // Script
            manifest.DefineScript("LaserOrchardSms").SetUrl("smsScript.js");
        }
    }
}