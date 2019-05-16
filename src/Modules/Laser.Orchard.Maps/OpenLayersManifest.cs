using Orchard.Environment.Extensions;
using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Maps {
    [OrchardFeature("Laser.Orchard.Maps.OpenLayers")]
    public class OpenLayersManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("OpenLayers")
                .SetUrl("ol.js");
            manifest.DefineStyle("OpenLayers")
                .SetUrl("ol.css");

            manifest.DefineScript("OL-Geocoder")
              .SetUrl("ol-geocoder.js");
            manifest.DefineStyle("OL-Geocoder")
                .SetUrl("ol-geocoder.css");
        }
    }
}