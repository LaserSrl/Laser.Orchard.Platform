using Orchard.Environment.Extensions;
using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Maps {
    [OrchardFeature("Laser.Orchard.Maps.OpenRoute")]
    public class OpenRouteManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("OpenRoute")
                .SetUrl("ors-client.js");
        }
    }
}