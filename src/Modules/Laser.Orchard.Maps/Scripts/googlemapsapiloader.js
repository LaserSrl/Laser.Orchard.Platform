function loadGoogleMaps(apiKey, lang) {
    (g => { var h, a, k, p = "The Google Maps JavaScript API", c = "google", l = "importLibrary", q = "__ib__", m = document, b = window; b = b[c] || (b[c] = {}); var d = b.maps || (b.maps = {}), r = new Set, e = new URLSearchParams, u = () => h || (h = new Promise(async (f, n) => { await (a = m.createElement("script")); e.set("libraries", [...r] + ""); for (k in g) e.set(k.replace(/[A-Z]/g, t => "_" + t[0].toLowerCase()), g[k]); e.set("callback", c + ".maps." + q); a.src = `https://maps.${c}apis.com/maps/api/js?` + e; d[q] = f; a.onerror = () => h = n(Error(p + " could not load.")); a.nonce = m.querySelector("script[nonce]")?.nonce || ""; m.head.append(a) })); d[l] ? console.warn(p + " only loads once. Ignoring:", g) : d[l] = (f, ...n) => r.add(f) && u().then(() => d[l](f, ...n)) })({
        key: apiKey,
        language: lang
        // Add other bootstrap parameters as needed, using camel case.
        // Use the 'v' parameter to indicate the version to load (alpha, beta, weekly, etc.)
    });
}

//$(document).ready(function () {
    var jsUrl = $("[googlemapsloader='googlemapsloader']").attr("src");
    var params = jsUrl.split("?");

    var apiKey = "";
    var lang = "";

    if (params.length > 1) {
        params = params[1].split("&");
        params.forEach(function (p) {
            var name_value = p.split("=");
            if (name_value.length > 1) {
                switch (name_value[0]) {
                    case "key":
                        apiKey = name_value[1];
                        break;

                    case "lang":
                        lang = name_value[1];
                        break;

                    default:
                        break;
                }
            }            
        });
    }

    if (apiKey != "") {
        loadGoogleMaps(apiKey, lang);
    }
//});