jQuery(function ($) {
    toggleDateTextBoxes();
    $('#operator').change(toggleDateTextBoxes);
});

function toggleDateTextBoxes() {
    $("#operator option:selected").each(function () {
        var val = $(this).val();

        if (val == 'Between') {
            $("#div-from").show();
            $("#div-to").show();
        } else if (val == 'LessThan') {
            $("#div-from").hide();
            $("#div-to").show();
        } else if (val == 'GreaterThan') {
            $("#div-from").show();
            $("#div-to").hide();
        }
    });
}