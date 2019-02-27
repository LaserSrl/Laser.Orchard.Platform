using Orchard.UI.Resources;

namespace Laser.Orchard.UserReactions {
    public class ResourceManifest : IResourceManifestProvider {

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            // CSS
            manifest.DefineStyle("Reactions").SetUrl("styles.min.css?v=2", "styles.min.css?v=2");
        }
    }
}
    