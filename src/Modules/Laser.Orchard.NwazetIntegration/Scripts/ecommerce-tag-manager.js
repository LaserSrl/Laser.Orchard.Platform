// create the scaffold for the object we will manage
window.ecommerceData = $.extend(true, {}, window.ecommerceData);
window.ecommerceData.detail = $.extend(true, {}, window.ecommerceData.detail);
window.ecommerceData.products = window.ecommerceData.products || [];
window.ecommerceData.impressions = window.ecommerceData.impressions || [];

$(function () {
    // This function will be executed before the DOM Ready event
    // is fired.

    // get the stuff we pushed around the page
    window.ecommerceData = $.extend(true, {}, window.ecommerceData);
    window.ecommerceData.detail = $.extend(true, {}, window.ecommerceData.detail);
    window.ecommerceData.detail.products = window.ecommerceData.detail.products || [];
    window.ecommerceData.impressions = window.ecommerceData.impressions || [];

    // put it all together and push it into the dataLayer
    // for Google Tag Manager. This dataLayer message should be
    // "used" on DOM ready events.
    var ecommerceObject = {};
    ecommerceObject.impressions = window.ecommerceData.impressions;
    if (window.ecommerceData.detail.products.length) {
        ecommerceObject.detail = window.ecommerceData.detail;
    }
    window.dataLayer = window.dataLayer || [];
    window.dataLayer.push({
        'ecommerce': ecommerceObject
    });
});