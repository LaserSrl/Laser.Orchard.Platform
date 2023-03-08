function InitializeGoogleMaps() {
    $("[data-map-provider='google']").each(function () {
        var mapId = $(this).attr("data-map-id");
        if (mapId) {
            //alert("Found map " + mapId);
            var functionName = "initialize" + mapId;
            window[functionName]();
        }
    });
}