// create the scaffold for the object we will manage
window.ecommerceData = $.extend(true, {}, window.ecommerceData);
window.ecommerceData.detail = $.extend(true, {}, window.ecommerceData.detail);
window.ecommerceData.detail.products = window.ecommerceData.detail.products || [];
window.ecommerceData.impressions = window.ecommerceData.impressions || [];
// array of objects used to track/manage changes to the shopping cart
window.ecommerceData.cart = $.extend(true, {}, window.ecommerceData.cart);
window.ecommerceData.cart.products = window.ecommerceData.cart.products || [];
// object used to track transactions/purchases
window.ecommerceData.purchase = $.extend(true, {}, window.ecommerceData.purchase);
window.ecommerceData.purchase.actionField = $.extend(true, {}, window.ecommerceData.purchase.actionField);
window.ecommerceData.purchase.products = window.ecommerceData.purchase.products || [];
// object used to track checkout steps
window.ecommerceData.checkout = $.extend(true, {}, window.ecommerceData.checkout);
window.ecommerceData.checkout.actionField = $.extend(true, {}, window.ecommerceData.checkout.actionField);
window.ecommerceData.checkout.products = window.ecommerceData.checkout.products || [];


$(function () {
    // This function will be executed before the DOM Ready event
    // is fired.

    // get the stuff we pushed around the page
    window.ecommerceData = $.extend(true, {}, window.ecommerceData);
    window.ecommerceData.detail = $.extend(true, {}, window.ecommerceData.detail);
    window.ecommerceData.detail.products = window.ecommerceData.detail.products || [];
    window.ecommerceData.impressions = window.ecommerceData.impressions || [];
    window.ecommerceData.cart = $.extend(true, {}, window.ecommerceData.cart);
    window.ecommerceData.cart.products = window.ecommerceData.cart.products || [];
    window.ecommerceData.purchase = $.extend(true, {}, window.ecommerceData.purchase);
    window.ecommerceData.purchase.actionField = $.extend(true, {}, window.ecommerceData.purchase.actionField);
    window.ecommerceData.purchase.products = window.ecommerceData.purchase.products || [];
    window.ecommerceData.checkout = $.extend(true, {}, window.ecommerceData.checkout);
    window.ecommerceData.checkout.actionField = $.extend(true, {}, window.ecommerceData.checkout.actionField);
    window.ecommerceData.checkout.products = window.ecommerceData.checkout.products || [];

    // put it all together and push it into the dataLayer
    // for Google Tag Manager. This dataLayer message should be
    // "used" on DOM ready events.
    var ecommerceObject = {};
    if (window.ecommerceData.impressions.length) {
        ecommerceObject.impressions = window.ecommerceData.impressions;
    }
    // if there is any product being displayed in a "detail" view:
    if (window.ecommerceData.detail.products.length) {
        ecommerceObject.detail = window.ecommerceData.detail;
    }
    // If we are displaying the results from a succesfull purchase
    if (!$.isEmptyObject(window.ecommerceData.purchase)
        && !$.isEmptyObject(window.ecommerceData.purchase.actionField)) {
        // we do not test the contents of purchase.products because in some special
        // cases it may be allowed to be empty
        ecommerceObject.purchase = window.ecommerceData.purchase;
    }
    // if we are displaying a checkout step
    if (!$.isEmptyObject(window.ecommerceData.checkout)
        && !$.isEmptyObject(window.ecommerceData.checkout.actionField)) {
        // we do not test the contents of purchase.products because in some special
        // cases it may be allowed to be empty
        ecommerceObject.checkout = window.ecommerceData.checkout;
    }
    // make sure dataLayer has been initialized
    window.dataLayer = window.dataLayer || [];
    // if there is anything to be sent immediately, push it
    if (!$.isEmptyObject(ecommerceObject)) {
        window.dataLayer.push({
            'ecommerce': ecommerceObject
        });
    }

    var productInArray = function (array, partId) {
        var product = {};
        for (i = 0; i < array.length; i++) {
            var currentProduct = array[i];
            if (currentProduct.partId == partId) {
                // found it
                product = currentProduct;
                break;
            }
        }
        return product;
    }

    var addRemoveAllCartChanges = function (el) {
        // get all set quantity elements
        var quantityElements = $(el).find(".quantity");
        if (quantityElements && quantityElements.length) {
            // found some
            // we will add to these arrays the products whose quantities changed
            var addedToCart = [];
            var removedFromCart = [];
            // go through all products in the cart
            for (i = 0; i < quantityElements.length; i++) {
                var prodId = $(quantityElements[i]).attr('data-cart-product-id');
                var currentQuantity = $(quantityElements[i]).val();
                if (prodId && currentQuantity) { // sanity check
                    var productChanged = productInArray(window.ecommerceData.cart.products, prodId);
                    if ($.isEmptyObject(productChanged)) {
                        // not found in the cart:
                        // this is a strange error condition that should not happen naturally
                        break;
                    }
                    // we have the product for this quantity
                    if (currentQuantity != productChanged.quantity) {
                        // quantity is different
                        var difference = currentQuantity - productChanged.quantity;
                        var prodCopy = {};
                        prodCopy = Object.assign(prodCopy, productChanged);
                        if (difference > 0) {
                            // added to cart
                            prodCopy.quantity = difference;
                            addedToCart.push(prodCopy);
                        } else {
                            // removed from cart
                            prodCopy.quantity = -difference;
                            removedFromCart.push(prodCopy);
                        }
                    }
                }
            }
            // raise the events for addition/removal from cart
            if (addedToCart.length) {
                window.dataLayer.push({
                    'event': 'addToCart',
                    'ecommerce': {
                        'add': {
                            'products': addedToCart
                        }
                    }
                });
            }
            if (removedFromCart.length) {
                window.dataLayer.push({
                    'event': 'removeFromCart',
                    'ecommerce': {
                        'remove': {
                            'products': removedFromCart
                        }
                    }
                });
            }
        }
    }
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
            var productAdded = productInArray(window.ecommerceData.detail.products, partId);
            if ($.isEmptyObject(productAdded)) {
                //not found among detail view. Search in summary views
                productAdded = productInArray(window.ecommerceData.impressions, partId);
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
            // id of the product we are trying to delete
            var prodId = $(this).attr('data-cart-product-id');
            // take it from the array of products in the cart
            var productRemoved = productInArray(window.ecommerceData.cart.products, prodId);
            if ($.isEmptyObject(productRemoved)) {
                // not found in the cart:
                // this is a strange error condition that should not happen naturally
                return;
            }
            // send a removed from cart event to tag manager
            if (productRemoved.quantity > 0) {
                // this check will allow us to avoid sending duplicate events
                window.dataLayer.push({
                    'event': 'removeFromCart',
                    'ecommerce': {
                        'remove': {
                            'products': [productRemoved]
                        }
                    }
                });
                productRemoved.quantity = 0;
            }
            console.log('removefromcart');
        })
    // CartUpdated
        .on("change", ".shoppingcart .quantity", function (e) {
            // $(this) is the input whose value for quantity changed
            // id of the product whose quantity changed
            var prodId = $(this).attr('data-cart-product-id');
            // take it from the array of products in the cart
            var productChanged = productInArray(window.ecommerceData.cart.products, prodId);
            if ($.isEmptyObject(productChanged)) {
                // not found in the cart:
                // this is a strange error condition that should not happen naturally
                return;
            }
            // when we will send the event corresponding to the quantity change
            // (either an add or remove) we will have to send the amount by which
            // the quantity changed, so we should not overwrite the "original" value
            // but rather save the "current" one, and make a difference when the "final"
            // update is called.
            var currentQuantity = $(this).val();
            productChanged.currentQuantity = currentQuantity;
            console.log('quantity changed to ' + currentQuantity);
        })
        // we cannot e.preventDefault in handling these events, because that would
        // break dynamic cart loading
        .on("submit", ".shopping-cart-container form", function (e) {
            // in $(this) we have the form
            addRemoveAllCartChanges(e.target);
            // post on a form 
            //console.log('cartupdated inside');
        })
        .on("submit", ".shoppingcart form", function (e) {
            // in $(this) we have the form
            addRemoveAllCartChanges(e.target);
            // post on a form 
            //console.log('cartupdated outside');
        })
        .on("submit", "form .shopping-cart-container", function (e) {
            // in $(this) we may not have the form
            addRemoveAllCartChanges(e.target);
            // post on a form 
            //console.log('cartupdated inside');
        })
        .on("submit", "form .shoppingcart", function (e) {
            // in $(this) we may not have the form
            addRemoveAllCartChanges(e.target);
            // post on a form 
            //console.log('cartupdated outside');
        })
        .on("click", "[data-product-id]", function (e) {
            // use this to track product clicks
            var prodId = $(this).attr('data-product-id');
            // if we did the page right, this product is among the impressions
            var productClicked = productInArray(window.ecommerceData.impressions, prodId);
            if ($.isEmptyObject(productClicked)) {
                // not found:
                // this is a strange error condition that should not happen naturally
                return;
            }
            // generate a product click event
            window.dataLayer.push({
                'event': 'productClick',
                'ecommerce': {
                    'click': {
                        'products': [productClicked]
                    }
                }
            });
        })
});

