using Laser.Orchard.Maps.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.UI.Resources;

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
            manifest.DefineScript("GoogleMapsApiCallback")
                .SetUrl("googlemapscallback.js");
            //manifest.DefineScript("GoogleMapsAPI")
            //    .SetUrl("https://maps.googleapis.com/maps/api/js?v=3&key=" + apiKey + languageQueryStringForGoogleMaps + "&callback=InitializeGoogleMaps")
            //    .AddAttribute("async", "async")
            //    .AddAttribute("defer", "defer")
            //    .SetDependencies("GoogleMapsApiCallback");
            manifest.DefineScript("GoogleMapsAPI")
                .SetUrl("googlemapsapiloader.js?key=" + apiKey + languageQueryStringForGoogleMaps)
                .AddAttribute("googlemapsloader", "googlemapsloader");
            manifest.DefineScript("GoogleMapsAPI_callback")
                .SetUrl("https://maps.googleapis.com/maps/api/js?v=3&key=" + apiKey + languageQueryStringForGoogleMaps + "&callback=InitializeMap")
                .AddAttribute("async", "async")
                .AddAttribute("defer", "defer");
            manifest.DefineScript("GoogleMapsAPIMarkerSpiderfier_callback")
                 .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/OverlappingMarkerSpiderfier/1.0.3/oms.min.js?spiderfier_callback=InitializeMap")
                 .AddAttribute("async", "async")
                 .AddAttribute("defer", "defer");

            // Obsolete script call for Places libraries
            manifest.DefineScript("GoogleMapsPlacesLib")
                .SetUrl("https://maps.googleapis.com/maps/api/js?v=3.exp&key=" + apiKey + languageQueryStringForGoogleMaps + "&libraries=places");

            // New script call for Places libraries
            manifest.DefineScript("GoogleMapsAPIPlaces")
                .SetUrl("googlemapsapiloader.js?key=" + apiKey + languageQueryStringForGoogleMaps + "&libraries=places")
                .AddAttribute("googlemapsloader", "googlemapsloader");

            // Obsolete script call for Geometry libraries
            manifest.DefineScript("GoogleMapsGeometryLib")
                .SetUrl("https://maps.googleapis.com/maps/api/js?v=3.exp&key=" + apiKey + languageQueryStringForGoogleMaps + "&libraries=geometry");

            // New script call for Geometry libraries
            manifest.DefineScript("GoogleMapsAPIGeometry")
                .SetUrl("googlemapsapiloader.js?key=" + apiKey + languageQueryStringForGoogleMaps + "&libraries=geometry")
                .AddAttribute("googlemapsloader", "googlemapsloader");

            manifest.DefineScript("MarkerClusterer")
                .SetUrl("MarkerClusterer.js");
            // CSS
            manifest.DefineStyle("GoogleMaps").SetUrl("GoogleMaps.css");

            // OSM Maps
            manifest.DefineScript("OpenLayersAPI")
              .SetUrl("https://www.openlayers.org/api/OpenLayers.js");

            manifest.DefineScript("OpenStreetMapAPI")
             .SetUrl("https://www.openstreetmap.org/openlayers/OpenStreetMap.js");

            // Leaflet
            manifest.DefineStyle("LeafletStyle").SetUrl("leaflet.css");

            manifest.DefineScript("LeafletScript").SetUrl("leaflet.js");
        }
    }
}