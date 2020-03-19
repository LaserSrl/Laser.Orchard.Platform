using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineScript("NwazetIntegration.Addresses").SetUrl("ship.js").SetDependencies("jQuery");

            // This script collects the information around the page and pushes it to the
            // dataLayer for google tag manager. The corresponding tag in Tag Managet to 
            // register the information should be configured with a DOM Ready trigger.
            manifest
                .DefineScript("NwazetIntegration.TagManager")
                .SetUrl("ecommerce-tag-manager.min.js", "ecommerce-tag-manager.js")
                .SetDependencies("jQuery");
        }
    }
}