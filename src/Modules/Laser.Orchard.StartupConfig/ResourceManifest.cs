using Orchard.UI.Resources;
namespace Laser.Orchard.StartupConfig {
    public class ResourceManifest : IResourceManifestProvider {

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            //manifest.DefineStyle("FontAwesome").SetUrl("font-awesome/css/font-awesome.min.css");
            manifest.DefineStyle("FontAwesome430").SetUrl("//maxcdn.bootstrapcdn.com/font-awesome/4.3.0/css/font-awesome.min.css");
            manifest.DefineStyle("FontAwesome430.ie7").SetUrl("//maxcdn.bootstrapcdn.com/font-awesome/4.3.0/css/font-awesome-ie7.min.css");

            //maxcdn.bootstrapcdn.com/font-awesome/4.3.0/css/font-awesome.min.css

            // color picker
            manifest.DefineScript("spectrum").SetUrl("spectrum.js").SetDependencies("jQuery");
            // tabulator (currently v3.4.4)
            manifest.DefineScript("tabulator").SetUrl("tabulator.min.js").SetDependencies("jQueryUI");
            manifest.DefineStyle("tabulator").SetUrl("tabulator.min.css");
        }
    }
}