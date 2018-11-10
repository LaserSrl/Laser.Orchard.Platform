using Orchard.UI.Resources;

namespace Laser.Orchard.Sharing {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineStyle("AddThis_Css3StyleButtons").SetUrl("AddThis.Css3StyleButtons.css");
            manifest.DefineStyle("AddThis_RetweetLikeShare").SetUrl("AddThis.RetweetLikeShare.css");
            manifest.DefineStyle("AddThis_TweetLikeShare").SetUrl("AddThis.TweetLikeShare.css");
        }
    }
}
