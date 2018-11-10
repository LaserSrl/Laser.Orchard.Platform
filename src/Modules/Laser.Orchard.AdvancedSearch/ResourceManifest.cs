using Orchard;
using Orchard.UI.Resources;
using Orchard.ContentManagement;


namespace Laser.Orchard.Maps {

    public class ResourceManifest : IResourceManifestProvider {

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            // CSS
            manifest.DefineStyle("AdvancedSearch").SetUrl("AdvancedSearch.css");
        }
    }
}