
using Orchard.Environment.Extensions;
using Orchard.UI.Resources;

namespace Laser.Orchard.Mobile {
    [OrchardFeature("Laser.Orchard.BannerAgent")]
    public class BannerAgentManifest : IResourceManifestProvider {

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("LaserOrchardSmartbanner").SetUrl("smartbanner.min.js");
            manifest.DefineStyle("LaserOrchardSmartbanner").SetUrl("smartbanner.min.css");
        }
    }
}