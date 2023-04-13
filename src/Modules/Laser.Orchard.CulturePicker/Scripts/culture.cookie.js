function getCultureCookie() {
    var decodedCookies = decodeURIComponent(document.cookie); //handle special characters
    var cookies = decodedCookies.split(";");
    for (var i = 0; i < cookies.length; i++) {
        var coo = cookies[i].trim();
        if (coo.indexOf("cultureData=") == 0) {
            return coo.substring(12, coo.length);
        }
    }
    return "";
}

function setCultureCookie(culture) {
    culture = culture.trim();
    if (culture != "") {
        var cvalue = "currentCulture=" + culture;
        var d = new Date();
        d.setTime(d.getTime() + (28 * 24 * 60 * 60 * 1000)); //expires in 4 weeks
        var expires = "expires=" + d.toGMTString();
        document.cookie = "cultureData=" + cvalue + ";" + expires + ";path=/;secure";
    }
}