// ResponsiveChanges.js
// Create mobile/tablet navigation (supports 3 levels)
// Duplicate main navigation in footer on mobile/small tablets
// Place first aside below #layout-content on mobile/small tablets

$(function () {

    // Function to collapse main navigation and create dropdown with icon
    function responsiveNavigation() {

        // Hide main navigation if visible
        $('#layout-navigation').hide();

        // Create collapsed navigation button with three lines and "Menu" text
        $('<a href="#" id="nav-collapsed"><span class="responsive-nav-bars"></span><span class="responsive-nav-bars"></span><span class="responsive-nav-bars"></span><span class="responsive-menu-text">Menu</span></a>').appendTo('.zone-header');

        // Toggle navigation visibility when collapsed navigation button is clicked
        $('#nav-collapsed').click(function () {
            $('#layout-navigation').toggle();
            if ($('#layout-navigation').is(':visible')) {
                $('#nav-collapsed').addClass('is-active');
            } else {
                $('#nav-collapsed').removeClass('is-active');
            }
        });

    };

    /* Append/remove responsive navigation based on breakpoints when document loads */
    if ($(window).width() <= 768) {
        responsiveNavigation();
    } else {
        $('#nav-collapsed').remove();
        $('#layout-navigation').show();
    };

    /* Append/remove responsive navigation based on breakpoints on resize event */
    $(window).resize(function () {
        if ($(window).width() <= 768 && $('#nav-collapsed').length != 1) {
            responsiveNavigation();
        } else if ($(window).width() > 768) {
            $('#nav-collapsed').remove();
            $('#layout-navigation').show();
        };
    });
    /* End responsive navigation */

    /**************************
    Optional changes common to mobile sites.
    ***************************/

    /* Duplicate main navigation in Footer (this comes in handy when scrolling to the bottom on mobile device) */
    $('#layout-navigation').clone().insertBefore('#footer-sig').addClass('footer-menu');

    /* Place first aside below layout-content on mobile browsers (aside's are generally less important than main content so this might make sense) */
    if ($(window).width() <= 768 && $('#aside-first').length == 1) {
        $('#aside-first').insertAfter('#layout-content');
    }
    $(window).resize(function () {
        if ($(window).width() <= 768 && $('#aside-first').length == 1) {
            $('#aside-first').insertAfter('#layout-content');
        }
        else if ($('#aside-first').length == 1) {
            $('#aside-first').insertBefore('#layout-content');
        }
    });
    /* End aside positioning */


});