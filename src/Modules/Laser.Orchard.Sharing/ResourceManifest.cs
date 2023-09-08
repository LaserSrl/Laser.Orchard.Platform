using Orchard.UI.Resources;

namespace Laser.Orchard.Sharing {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("Laser_Orchard_Sharing").SetCdn("https://static.addtoany.com/menu/page.js").SetUrl("page.min.js");
        }
    }
}
