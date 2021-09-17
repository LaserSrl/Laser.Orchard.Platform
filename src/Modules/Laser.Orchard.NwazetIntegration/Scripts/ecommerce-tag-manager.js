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

// Object used to track checkout with GA4
window.GA4Data = $.extend(true, {}, window.GA4Data);
window.GA4Data.event = window.GA4Data.event || '';
window.GA4Data.ecommerce = $.extend(true, {}, window.GA4Data.ecommerce);
window.GA4Data.ecommerce.items = window.GA4Data.ecommerce.items || [];
// Additional params of ecommerce (e.g. "purchase" event requires / accepts more parameters)
window.GA4Data.ecommerce.transaction_id = window.GA4Data.ecommerce.transaction_id || '';
window.GA4Data.ecommerce.affiliation = window.GA4Data.ecommerce.affiliation || '';
window.GA4Data.ecommerce.value = window.GA4Data.ecommerce.value || '';
window.GA4Data.ecommerce.tax = window.GA4Data.ecommerce.tax || '';
window.GA4Data.ecommerce.shipping = window.GA4Data.ecommerce.shipping || '';
window.GA4Data.ecommerce.currency = window.GA4Data.ecommerce.currency || '';
window.GA4Data.ecommerce.coupon = window.GA4Data.ecommerce.coupon || '';
window.GA4Data.ecommerce.shipping_tier = window.GA4Data.ecommerce.shipping_tier || '';
window.GA4Data.ecommerce.payment_type = window.GA4Data.ecommerce.payment_type || '';

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

    window.GA4Data = $.extend(true, {}, window.GA4Data);
    window.GA4Data.event = window.GA4Data.event || '';
    window.GA4Data.ecommerce = $.extend(true, {}, window.GA4Data.ecommerce);
    window.GA4Data.ecommerce.items = window.GA4Data.ecommerce.items || [];
    // Additional params of ecommerce (e.g. "purchase" event requires / accepts more parameters)
    window.GA4Data.ecommerce.transaction_id = window.GA4Data.ecommerce.transaction_id || '';
    window.GA4Data.ecommerce.affiliation = window.GA4Data.ecommerce.affiliation || '';
    window.GA4Data.ecommerce.value = window.GA4Data.ecommerce.value || '';
    window.GA4Data.ecommerce.tax = window.GA4Data.ecommerce.tax || '';
    window.GA4Data.ecommerce.shipping = window.GA4Data.ecommerce.shipping || '';
    window.GA4Data.ecommerce.currency = window.GA4Data.ecommerce.currency || '';
    window.GA4Data.ecommerce.coupon = window.GA4Data.ecommerce.coupon || '';
    window.GA4Data.ecommerce.shipping_tier = window.GA4Data.ecommerce.shipping_tier || '';
    window.GA4Data.ecommerce.payment_type = window.GA4Data.ecommerce.payment_type || '';

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


    var GA4Object = {};
    if (!$.isEmptyObject(window.GA4Data)
        && window.GA4Data.event != ''
        && !$.isEmptyObject(window.GA4Data.ecommerce)
        && window.GA4Data.ecommerce.items.length) {
        GA4Object.event = window.GA4Data.event;
        GA4Object.ecommerce = window.GA4Data.ecommerce;

        // Additional params of ecommerce (e.g. "purchase" event requires / accepts more parameters)
        if (GA4Object.event == 'purchase') {
            GA4Object.ecommerce.transaction_id = window.GA4Data.ecommerce.transaction_id || '';
            GA4Object.ecommerce.affiliation = window.GA4Data.ecommerce.affiliation || '';
            GA4Object.ecommerce.value = window.GA4Data.ecommerce.value || '';
            GA4Object.ecommerce.tax = window.GA4Data.ecommerce.tax || '';
            GA4Object.ecommerce.shipping = window.GA4Data.ecommerce.shipping || '';
            GA4Object.ecommerce.currency = window.GA4Data.ecommerce.currency || '';
            GA4Object.ecommerce.coupon = window.GA4Data.ecommerce.coupon || '';
        } else if (GA4Object.event == 'add_shipping_info') {
            GA4Object.ecommerce.currency = window.GA4Data.ecommerce.currency || '';
            GA4Object.ecommerce.value = window.GA4Data.ecommerce.value || 0;
            GA4Object.ecommerce.coupon = window.GA4Data.ecommerce.coupon || '';
            GA4Object.ecommerce.shipping_tier = window.GA4Data.ecommerce.shipping_tier || '';
        } else if (GA4Object.event == 'add_payment_info') {
            GA4Object.ecommerce.currency = window.GA4Data.ecommerce.currency || '';
            GA4Object.ecommerce.value = window.GA4Data.ecommerce.value || 0;
            GA4Object.ecommerce.coupon = window.GA4Data.ecommerce.coupon || '';
            GA4Object.ecommerce.payment_type = window.GA4Data.ecommerce.payment_type || '';
        }
    }

    // make sure dataLayer has been initialized
    window.dataLayer = window.dataLayer || [];
    // if there is anything to be sent immediately, push it
    if (!$.isEmptyObject(ecommerceObject)) {
        window.dataLayer.push({
            'ecommerce': ecommerceObject
        });
    }

    if (!$.isEmptyObject(GA4Object)) {
        window.dataLayer.push(GA4Object);
    }

    // view_item event, pushed if there is any element in window.ecommerceData.datail.products.
    // I can use the same array because, when loading, I load the data in the new format using IGAProductVM interface.
    if (window.ecommerceData.detail.products.length) {
        var GA4_view_item = {};
        GA4_view_item.event = "view_item";
        GA4_view_item.ecommerce = {};
        GA4_view_item.ecommerce.items = window.ecommerceData.detail.products;
        window.dataLayer.push(GA4_view_item);
    }

    // view_item_list event, pushed if there is any element in window.ecommerceData.impressions.
    // I can use the same array because, when loading, I load the data in the new format using IGAProductVM interface.
    if (window.ecommerceData.impressions.length) {
        var GA4_view_item_list = {};
        GA4_view_item_list.event = "view_item_list";
        GA4_view_item_list.ecommerce = {};
        GA4_view_item_list.ecommerce.items = window.ecommerceData.impressions;
        window.dataLayer.push(GA4_view_item_list);
    }

    var productInArray = function (array, partId) {
        var product = {};
        for (var i = 0; i < array.length; i++) {
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
            for (var i = 0; i < quantityElements.length; i++) {
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
                if (window.useGA4) {
                    window.dataLayer.push({
                        event: 'add_to_cart',
                        ecommerce: {
                            items: addedToCart
                        }
                    });
                } else {
                    window.dataLayer.push({
                        'event': 'addToCart',
                        'ecommerce': {
                            'add': {
                                'products': addedToCart
                            }
                        }
                    });
                }
            }
            if (removedFromCart.length) {
                if (window.useGA4) {
                    window.dataLayer.push({
                        event: 'remove_from_cart',
                        ecommerce: {
                            items: removedFromCart
                        }
                    });
                } else {
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
        .on("nwazet.addedtocart", "form.addtocart", function (e, context) {
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
            var quantity = context.movedQuantity;
            productAdded.quantity = quantity;

            if (window.useGA4) {
                window.dataLayer.push({
                    event: 'add_to_cart',
                    ecommerce: {
                        items: [productAdded]
                    }
                });
            } else {
                window.dataLayer.push({
                    'event': 'addToCart',
                    'ecommerce': {
                        'add': {
                            'products': [productAdded]
                        }
                    }
                });
            }
        })
        // RemoveFromCart1: use the event from shoppingcart.js
        .on("nwazet.removefromcart", "form.addtocart", function (e) {
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
                if (window.useGA4) {
                    window.dataLayer.push({
                        event: 'remove_from_cart',
                        ecommerce: {
                            items: [productRemoved]
                        }
                    });
                } else {
                    // this check will allow us to avoid sending duplicate events
                    window.dataLayer.push({
                        'event': 'removeFromCart',
                        'ecommerce': {
                            'remove': {
                                'products': [productRemoved]
                            }
                        }
                    });
                }
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
        .on("submit", ".shoppingcart form", function (e) {
            // in $(this) we may not have the form
            addRemoveAllCartChanges(e.target);
            // post on a form 
            //console.log('cartupdated outside');
        })
        .on("click", "[data-analytics-product-id]", function (e) {
            // use this to track product clicks
            var prodId = $(this).attr('data-analytics-product-id');
            // if we did the page right, this product is among the impressions
            var productClicked = productInArray(window.ecommerceData.impressions, prodId);
            if ($.isEmptyObject(productClicked)) {
                // not found:
                // if I'm in the shopping cart, I need to search the product in the right array.
                productClicked = productInArray(window.ecommerceData.cart.products, prodId);
                if ($.isEmptyObject(productClicked)) {
                    return;
                }
            }
            if (window.useGA4) {
                window.dataLayer.push({
                    event: 'select_item',
                    ecommerce: {
                        items: [productClicked]
                    }
                });
            } else {
                // generate a product click event
                window.dataLayer.push({
                    'event': 'productClick',
                    'ecommerce': {
                        'click': {
                            'products': [productClicked]
                        }
                    }
                });
            }
        })
        .on("submit", 'form[data-analytics-form="review"]', function (e) {
            // This event represents the click on the payment button of each payment provider (before the actual payment, it's the payment provider selection).
            // For this reason, I only need to check if a valid pos service has been selected.
            // Other info required by GA4 event are read from the global GA4Data object.
            // SelectedPosService isn't in the formData object, because it's not an input control, it's the value of the original submit button clicked.
            var posService = e.originalEvent.submitter.value;
            if (posService) {
                window.dataLayer.push({
                    event: 'add_payment_info',
                    currency: window.GA4Data.ecommerce.currency,
                    value: window.GA4Data.ecommerce.value,
                    coupon: window.GA4Data.ecommerce.coupon,
                    payment_type: posService,
                    ecommerce: {
                        items: window.GA4Data.ecommerce.items || []
                    }
                });
            }
        })
});