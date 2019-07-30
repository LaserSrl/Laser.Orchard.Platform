function injectThemeHidingCss() {
    $('head').append('<link href="/Orchard/Modules/Laser.Orchard.StartupConfig/styles/contentPickerCreation/ContentPickerThemeHiding.css" rel="stylesheet" type="text/css" />');
}

const urlParams = new URLSearchParams(window.location.search);
const callback = urlParams.get('callback');

if (callback != null && callback.startsWith("_contentpickercreate_")) {
    injectThemeHidingCss();

    const cpfName = urlParams.get('namecpfield');
    const ctName = urlParams.get('selectedct');

    window.sessionStorage.setItem("cpfCreationSession", "true");
    window.sessionStorage.setItem("cpfName", cpfName);
    window.sessionStorage.setItem("ctName", ctName);
} else if (window.sessionStorage.getItem("cpfCreationSession") != null) {
    injectThemeHidingCss();
}