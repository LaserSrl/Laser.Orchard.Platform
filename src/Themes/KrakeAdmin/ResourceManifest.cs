using Orchard.UI.Resources;

namespace KrakeAdmin {
    public class ResourceManifest : IResourceManifestProvider {

        public const string BaseAdmin = "BaseAdmin";
        public const string KrakeAdmin = "KrakeAdmin";
        public const string KrakeNavigation = "KrakeNavigation";
        public const string Bootstrap = "Bootstrap";
        public const string Site = "Site";
        public const string TooltipImagezoom = "TooltipsZoomimage";

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineStyle(BaseAdmin).SetUrl("baseadmin.min.css?v1.0", "baseadmin.css?v1.0");
            manifest.DefineStyle(KrakeAdmin).SetUrl("krakeadmin.min.css?v2.6", "krakeadmin.css?v2.6");
            manifest.DefineStyle(KrakeNavigation).SetUrl("krake-navicon.min.css?v1.4", "krake-navicon.css?v1.4");
            manifest.DefineScript(TooltipImagezoom).SetUrl("TooltipsZoomimage.js", "TooltipsZoomimage.js");
        }
    }
}