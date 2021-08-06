using Laser.Orchard.Maps.Models;
using Orchard;
using Orchard.UI.Resources;
using Orchard.ContentManagement;


namespace Laser.Orchard.Maps {

    public class ResourceManifest : IResourceManifestProvider {
        private readonly IOrchardServices _orchardServices;
        public ResourceManifest(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public void BuildManifests(ResourceManifestBuilder builder) {
            var apiKey = "missing-api-key";
            var mapsSettings = _orchardServices.WorkContext.CurrentSite.As<MapsSiteSettingsPart>();
            if (!string.IsNullOrWhiteSpace(mapsSettings.GoogleApiKey)) {
                apiKey = mapsSettings.GoogleApiKey;
            }
            bool keepCultureConsistent = mapsSettings.KeepCultureConsistentWithContext;
            string languageQueryStringForGoogleMaps = (keepCultureConsistent ? "&language=" + _orchardServices.WorkContext.CurrentCulture : "");
            var manifest = builder.Add();
            manifest.DefineScript("LaserOrchardMaps")
                .SetUrl("maps.js");
            // Google Maps
            //Scripts
            manifest.DefineScript("GoogleMapsAPI")
                .SetUrl("https://maps.googleapis.com/maps/api/js?v=3&key=" + apiKey + languageQueryStringForGoogleMaps);
            manifest.DefineScript("GoogleMapsAPI_callback")
                .SetUrl("https://maps.googleapis.com/maps/api/js?v=3&key=" + apiKey + languageQueryStringForGoogleMaps + "&callback=InitializeMap").AddAttribute("async", "async").AddAttribute("defer", "defer");
            manifest.DefineScript("GoogleMapsAPIMarkerSpiderfier_callback")
                 .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/OverlappingMarkerSpiderfier/1.0.3/oms.min.js?spiderfier_callback=InitializeMap").AddAttribute("async", "async").AddAttribute("defer", "defer");

            manifest.DefineScript("GoogleMapsPlacesLib")
                .SetUrl("https://maps.googleapis.com/maps/api/js?v=3.exp&key=" + apiKey + languageQueryStringForGoogleMaps + "&libraries=places");

            manifest.DefineScript("GoogleMapsGeometryLib")
                .SetUrl("https://maps.googleapis.com/maps/api/js?v=3.exp&key=" + apiKey + languageQueryStringForGoogleMaps + "&libraries=geometry");

            manifest.DefineScript("MarkerClusterer")
                .SetUrl("MarkerClusterer.js");
            // CSS
            manifest.DefineStyle("GoogleMaps").SetUrl("GoogleMaps.css");

            // OSM Maps
            manifest.DefineScript("OpenLayersAPI")
              .SetUrl("http://www.openlayers.org/api/OpenLayers.js");

            manifest.DefineScript("OpenStreetMapAPI")
             .SetUrl("http://www.openstreetmap.org/openlayers/OpenStreetMap.js");
        }
    }
}