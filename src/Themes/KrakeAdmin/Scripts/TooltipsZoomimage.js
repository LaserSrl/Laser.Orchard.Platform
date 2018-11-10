
$( function() {
    $( document ).tooltip({
        items: "[data-zoomimage]",
        content: function() {
            var element = $( this );
            if (element.is("[data-zoomimage]")) {
                return "<img src='" + element.data("zoomimage") + "'>";
            }
            if ( element.is( "img" ) ) {
                return element.attr( "alt" );
            }
        }
    });
} );