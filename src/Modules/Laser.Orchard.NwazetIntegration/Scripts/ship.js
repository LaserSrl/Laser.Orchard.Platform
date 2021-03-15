var global_CopyingAddresses = false;
var global_ChangingProgrammatically = false;

$(function () {
    var addressForm = $("#address-form"),
        errorZone = $(".ship-errors"),
        toggleCheckbox = $("#toggle-billing-address");
    toggleCheckbox
        .change(function () {
            $(".billing-address").toggle($(this).val());
            global_CopyingAddresses = true;
            if ($(this).val() === "on" || $(this).val() === "true") {
                $('input[name^="shippingAddressVM."]').each(function () {
                    shippingAddressHasChanged($(this));
                });
                shippingAddressHasChanged($('select[name="shippingAddressVMListAddress"]'));
                $('select[name^="shippingAddressVM."]').each(function () {
                    shippingAddressHasChanged($(this));
                });
            }

            // trigger a custom visibility event
            $(".billing-address").trigger('visibilityChanged');
            global_CopyingAddresses = false;
        });
    if (toggleCheckbox.prop("checked")) {
        // if the checkbox begins as checked
        $(".billing-address").hide();
    }
    $('input[name^="shippingAddressVM."]')
        .change(function () {
            shippingAddressHasChanged($(this));
        });
    $('select[name="shippingAddressVMListAddress"]')
        .change(function () {
            shippingAddressHasChanged($(this));
        });
    $('select[name^="shippingAddressVM."]')
        .change(function () {
            shippingAddressHasChanged($(this));
        });


    if (!("preventRequiredMark" in window) || !preventRequiredMark) {
        addressForm.find(".required").after(
            $("<span class='error-indicator' title='" + required + "'>*</span>"));
    }

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
                if (alreadyRequired.indexOf(label) === -1) {
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
            //if (firstErrorElement) {
            //    firstErrorElement.scrollIntoView();
            //}
            return false;
        }
    });
});

function shippingAddressHasChanged(sender) {
    var toggleCheckbox = $("#toggle-billing-address");

    if (!toggleCheckbox.prop("checked")) return;
    var input = sender,
        name = input.attr("name").indexOf(".") > 0 ? input.attr("name").substr("shippingAddressVM.".length) : input.attr("name").substr("shippingAddressVM".length);

    if (sender.prop("tagName").toLowerCase() == "input") {
        $('input[name="billingAddressVM.' + name + '"]')
            .val(input.val())
            .trigger('change');
    } else if (sender.prop("tagName").toLowerCase() == "select") {
        var other;
        if (name != "ListAddress") {
            other = $('select[name="billingAddressVM.' + name + '"]');
            var newValue = input.val();
            if (other.find('option[value="' + newValue + '"]').length == 0) {
                // add and select the option
                newOption = new Option(
                    input.find('option[value=' + newValue + ']').text(), //text
                    newValue, //value
                    true, //defaultSelected
                    true); //selected
                other.append(newOption);
            }
        } else {
            other = $('select[name="billingAddressVMListAddress"]');
            newValue = "-1";
        }

        other.val(newValue);
        if (newValue != "-1" || name != 'ListAddress') {
            other.trigger('change');
        }
    }
}
