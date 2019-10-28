using Orchard.Environment.Extensions;
using Orchard.UI.Resources;

namespace Laser.Orchard.StartupConfig.TinyMceEnhancement {
    [OrchardFeature("Laser.Orchard.StartupConfig.TinyMceEnhancement")]
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            // tinymce enhancement: replace standard orchard init with custom init
            manifest.DefineScript("OrchardTinyMce").SetUrl("laser-tinymce.js").SetDependencies("TinyMce").SetVersion("1.1");
        }
    }
}