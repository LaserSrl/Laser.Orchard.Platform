function injectThemeHidingCss(basepath) {
    if (basepath != "" && basepath) {
        if (!basepath.startsWith("/")) {
            basepath = "/" + basepath;
        }
        if (!basepath.endsWith("/")) {
            basepath = basepath + "/";
        }
    } else {
        basepath = "/";
    }
    $('head').append('<link href="' + basepath + 'Modules/Laser.Orchard.StartupConfig/styles/contentPickerCreation/ContentPickerThemeHiding.css" rel="stylesheet" type="text/css" />');
}

function enableSelectButton(contentType) {
    if (window.sessionStorage.getItem("ctName") == contentType) {
        $('#cp-creation').show();
    } else {
        $('#cp-creation').hide();
    }
}