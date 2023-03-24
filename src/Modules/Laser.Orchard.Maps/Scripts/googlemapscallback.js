// This function is called as a callback after loading Google Maps Api script.
// It searches for a specific attribute (data-map-provider) inside the page.
// If the attribute value is "google", the map needs to be initialized after page is loaded.
// To be properly initialized, it is required that "data-map-id" attribute contains a proper value.
// The value is then used to execute the initialize function for the correlated map.
// If data-map-provider is not present or doesn't container the value "google", 
// the map needs to be initialized by another script(e.g.the pression of a button).
function InitializeGoogleMaps() {
    $("[data-map-provider='google']").each(function () {
        var mapId = $(this).attr("data-map-id");
        if (mapId) {
            var functionName = "initialize" + mapId;
            $(document).ready(function () {
                window[functionName]();
            });
        }
    });
}