jQuery(function ($) {
    $(document)
        .on("submit", "[data-form-role='addtocart']", function (e) {
            // replace button with spinner and disabled button
            $(this).find("[data-cart-button-state='button']").hide();
            $(this).find("[data-cart-button-state='spinner']").show();

        })
        .on("nwazet.addedtocart", "[data-form-role='addtocart']", function (e, context) {
            // restored button
            $(this).find("[data-cart-button-state='spinner']").hide();
            $(this).find("[data-cart-button-state='button']").show();
        });
});