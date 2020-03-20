// create the scaffold for the object we will manage
window.ecommerceData = $.extend(true, {}, window.ecommerceData);
window.ecommerceData.detail = $.extend(true, {}, window.ecommerceData.detail);
window.ecommerceData.products = window.ecommerceData.products || [];
window.ecommerceData.impressions = window.ecommerceData.impressions || [];
//

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
    // if there is any product being displayed in a "detail" view:
    if (window.ecommerceData.detail.products.length) {
        ecommerceObject.detail = window.ecommerceData.detail;
    }
    // make sure dataLayer has been initialized
    window.dataLayer = window.dataLayer || [];
    window.dataLayer.push({
        'ecommerce': ecommerceObject
    });

    //register handlers

    // Add to cart:
    // There are 2 ways to add products to the cart:
    //   1. Clicking an AddToCart button and triggering the events form shoppingcart.js
    //   2. Invoking an update on the shopping cart page where a quantity has been increased
    // Remove from cart:
    // There are 2 ways to remove products from the cart:
    //   1. Deleting the entire line from the cart by triggering the events in shoppingcart.js
    //   2. Invoking an update on the shopping cart page where a quantity has been decreased

    // AddToCart1: This uses the event launched by the shoppingcart.js script
    $(document)
        .on("nwazet.addedtocart", "form.addtocart", function (e) {
            // in $(this) we have the form
            var formData = $(this)
                // serialize to an array of oblects like {name: '', value:''}
                .serializeArray()
                // reduce the array to an object with the named properties
                .reduce(function (o, v) {
                    o[v.name] = v.value;
                    return o;
                }, {});
            // given the stuff in the form, get the information to send 
            // to the tag manager: the product and the quantity added.
            var partId = formData.id;
            // get the product with that id from any of our lists
            var productAdded = {};
            for (i = 0; i < window.ecommerceData.detail.products.length; i++) {
                var currentProduct = window.ecommerceData.detail.products[i];
                if (currentProduct.partId == partId) {
                    // found it
                    productAdded = currentProduct;
                    break;
                }
            }
            if ($.isEmptyObject(productAdded)) {
                //not found among detail view. Search in summary views
                for (i = 0; i < window.ecommerceData.impressions.length; i++) {
                    var currentProduct = window.ecommerceData.impressions[i];
                    if (currentProduct.partId == partId) {
                        // found it
                        productAdded = currentProduct;
                        break;
                    }
                }
            }
            if ($.isEmptyObject(productAdded)) {
                // not even found there:
                // this is a strange error condition that should not happen naturally
                return;
            }
            var quantity = formData.quantity;
            productAdded.quantity = quantity;
            window.dataLayer.push({
                'event': 'addToCart',
                'ecommerce': {
                    'add': {
                        'products': [productAdded]
                    }
                }
            });
        })
    // RemoveFromCart1: use the event from shoppingcart.js
        .on("nwazet.removefromcart", ".shoppingcart .delete", function (e) {
            // $(this) is the element that was clicked to trigger the event
            // (generally an anchor tag)

            console.log('removefromcart');
        })
    // CartUpdated
        .on("nwazet.cartupdated", function (e) {

            console.log('cartupdated');
        })
});

