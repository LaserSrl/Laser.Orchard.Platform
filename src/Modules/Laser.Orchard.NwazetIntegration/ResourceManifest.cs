using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest
                .DefineScript("NwazetIntegration.Addresses")
                .SetUrl("ship.min.js?v=1.4", "ship.js?v=1.4")
                .SetDependencies("jQuery");

            manifest
                .DefineScript("NwazetIntegration.AddressConfiguration")
                .SetUrl("address-configuration.min.js?v=1.4", "address-configuration.js?v=1.4")
                .SetDependencies("jQuery");

            manifest
                .DefineStyle("NwazetIntegration.Admin")
                .SetUrl("admin-style.min.css", "admin-style.css");

            // This script collects the information around the page and pushes it to the
            // dataLayer for google tag manager. The corresponding tag in Tag Managet to 
            // register the information should be configured with a DOM Ready trigger.
            manifest
                .DefineScript("NwazetIntegration.TagManager")
                .SetUrl("ecommerce-tag-manager.min.js?v=1.0", "ecommerce-tag-manager.js?v=1.0")
                .SetDependencies("jQuery");

            // These scripts are used for autocomplete in the selection of territories
            manifest
                .DefineScript("NwazetIntegration.tagit.js")
                .SetUrl("tagit.min.js", "tagit.js")
                .SetDependencies("jQueryUI");
            manifest
                .DefineScript("NwazetIntegration.TerritoriesAutoComplete")
                .SetUrl("territories-autocomplete.min.js", "territories-autocomplete.js")
                .SetDependencies("NwazetIntegration.tagit.js");
        }
    }
}