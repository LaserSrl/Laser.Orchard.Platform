$(function () {
    var addressForm = $("#address-form"),
        errorZone = $(".ship-errors"),
        toggleCheckbox = $("#toggle-billing-address");
    toggleCheckbox
        .change(function () {
            $(".billing-address").toggle($(this).val());
            
            if ($(this).val() == "on") {
                $('input[name^="shippingAddressVM."]').each(function () {
                    var input = $(this),
                        name = input.attr("name").substr(18);
                    $('input[name="billingAddressVM.' + name + '"]')
                        .val(input.val())
                        .trigger('change');
                });
                $('select[name^="shippingAddressVM."]').each(function () {
                    var input = $(this),
                        name = input.attr("name").substr(18);
                    $('select[name="billingAddressVM.' + name + '"]')
                        .val(input.val())
                        .trigger('change');
                });
            }

            // trigger a custom visibility event
            $(".billing-address").trigger('visibilityChanged');
        });
    $('input[name^="shippingAddressVM."]')
        .change(function () {
            if (!toggleCheckbox.prop("checked")) return;
            var input = $(this),
                name = input.attr("name").substr(18);
            $('input[name="billingAddressVM.' + name + '"]')
                .val(input.val())
                .trigger('change');
        });
    $('select[name^="shippingAddressVM."]')
        .change(function () {
            if (!toggleCheckbox.prop("checked")) return;
            var input = $(this),
                name = input.attr("name").substr(18);
            $('select[name="billingAddressVM.' + name + '"]')
                .val(input.val())
                .trigger('change');
        });
    addressForm.find(".required").after(
        $("<span class='error-indicator' title='" + required + "'>*</span>"));
    addressForm.submit(function (e) {
        var validated = true,
            firstErrorElement,
            alreadyRequired = [];
        addressForm.find(".required").each(function () {
            var requiredField = $(this);
            if ((requiredField.is('input') || requiredField.is('select'))
                && !requiredField.val()) {
                validated = false;
                var id = requiredField.attr("id"),
                    label = addressForm.find("label[for='" + id + "']").html();
                requiredField.addClass("required-error");
                if (alreadyRequired.indexOf(label) == -1) {
                    errorZone.show().append(
                        $("<div></div>").html(requiredFormat.replace("{0}", label))
                    );
                    alreadyRequired.push(label);
                }
                if (!firstErrorElement) {
                    firstErrorElement = this;
                    firstErrorElement.focus();
                }
            } else {
                requiredField.removeClass("required-error");
            }
        });
        if (!validated) {
            e.preventDefault();
            if (firstErrorElement) {
                firstErrorElement.scrollIntoView();
            }
            return false;
        }
    });
});
