
(function ($) {

    var populate = function (el, prefix) {
        var pos = 1;
    };

    $('.mv-mb-nav-menu > ol').nestedSortable({
        disableNesting: 'no-nest',
        forcePlaceholderSize: true,
        handle: 'div',
        helper: 'clone',
        items: 'li',
        maxLevels: 16,
        opacity: 1,
        placeholder: 'mv-mb-nav-placeholder',
        revert: 50,
        tabSize: 30,
        tolerance: 'pointer',
        toleranceElement: '> div',

        stop: function (event, ui) {
            // update all positions whenever a menu item was moved
            populate(this, '');
            $('#mb-save-message').show();
            // visualizza il pulsante di aggiorna
            $('#mb-update-button').show();

            //// display a message on leave if changes have been made
            //window.onbeforeunload = function (e) {
            //    return ConfermaUscita;
            //};

            //// cancel leaving message on save
            //$('#saveButton').click(function (e) {
            //    window.onbeforeunload = function () { };
            //});

            //// cancel leaving message on save
            //$('#primaryAction').click(function (e) {
            //    window.onbeforeunload = function () { };
            //});

            //$('#submit.Save').click(function (e) {
            //    window.onbeforeunload = function () { };
            //});

        }
    });

    

})(jQuery);
