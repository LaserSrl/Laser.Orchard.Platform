using Orchard.UI.Resources;

namespace Laser.Orchard.Highlights {

  public class ResourceManifest : IResourceManifestProvider {

    public void BuildManifests(ResourceManifestBuilder builder) {
      var manifest = builder.Add();
        //TODO: HS => Creare modulo per plugin jQuery e css corredati per essere utilizzati da più moduli e non solo da Highlights
      manifest.DefineScript("Highlights").SetUrl("Highlights.js").SetDependencies("jQuery");
      manifest.DefineStyle("Highlights").SetUrl("slider-Highlights.css");
    }
  }
}