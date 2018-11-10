using Orchard.UI.Resources;

namespace Laser.Orchard.Meteo {

    public class ResourceManifest : IResourceManifestProvider {

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            // CSS
            manifest.DefineStyle("Jssor-Meteo").SetUrl("meteo-jssor.css");
        }
    }
}