using Orchard.UI.Resources;

namespace KrakeAdmin {
    public class ResourceManifest : IResourceManifestProvider {

        public const string KrakeAdmin = "KrakeAdmin";
        public const string Krake = "Krake";
        public const string KrakeNavigation = "KrakeNavigation";
        public const string Bootstrap = "Bootstrap";
        public const string Site = "Site";
        public const string TooltipImagezoom = "TooltipsZoomimage";

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineStyle(KrakeAdmin).SetUrl("krakeadmin.min.css?v1.4", "krakeadmin.css?v1.4");
            manifest.DefineStyle(Krake).SetUrl("krake.min.css?v1.4", "krake.css?v1.4");
            manifest.DefineStyle(KrakeNavigation).SetUrl("krake-navicon.min.css?v1.4", "krake-navicon.css?v1.4");
            //manifest.DefineStyle(Bootstrap).SetUrl("bootstrap.css", "bootstrap.css");
            manifest.DefineStyle(Site).SetUrl("site.css", "site.css");

            manifest.DefineScript(TooltipImagezoom).SetUrl("TooltipsZoomimage.js", "TooltipsZoomimage.js");
            //manifest.DefineScript(Bootstrap).SetUrl("bootstrap.js", "bootstrap.js");
            //Style.Include("font-awesome.min.css").AtHead();
            //Style.Include("iconic-font.min.css").AtHead();
            //Style.Include("https://fonts.googleapis.com/css?family=Roboto:400,700,500,300,100,900").AtHead();

        }
    }
}