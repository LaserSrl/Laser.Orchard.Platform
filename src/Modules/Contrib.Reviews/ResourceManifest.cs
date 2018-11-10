using Orchard.UI.Resources;

namespace Contrib.Reviews {

    public class ResourceManifest : IResourceManifestProvider {

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            //Scripts
            manifest.DefineScript("Contrib_Stars").SetUrl("Contrib.Stars.js").SetDependencies("jQuery");

            //Styles
            manifest.DefineStyle("Contrib_Stars").SetUrl("Contrib.Stars.css");
        }
    }
}