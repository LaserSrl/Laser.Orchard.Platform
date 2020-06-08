using Orchard.Environment.Extensions;
using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ContentExtension {
    [OrchardFeature("Laser.Orchard.ContentExtension.DynamicProjection")]
    public class DynamicProjectionResourceManifest : IResourceManifestProvider {

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("AdminDynamicProjections").SetUrl("admin-dynamic-projections.min.css", "admin-dynamic-projections.css");
        }
    }
}