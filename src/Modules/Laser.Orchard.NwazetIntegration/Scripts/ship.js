﻿var global_CopyingAddresses = false;
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
                    var input = $(this),
                        name = input.attr("name").substr(18);
                    $('input[name="billingAddressVM.' + name + '"]')
                        .val(input.val())
                        .trigger('change');
                });
                $('select[name^="shippingAddressVM."]').each(function () {
                    var input = $(this),
                        name = input.attr("name").substr(18);
                    var other = $('select[name="billingAddressVM.' + name + '"]');
                    var newValue = input.val();
                    if (other.find('option[value="' + newValue + '"]').length === 0) {
                        // add and select the option
                        var newOption =
                            new Option(
                                input.find('option[value=' + newValue + ']').text(), //text
                                newValue, //value
                                true, //defaultSelected
                                true); //selected
                        other.append(newOption);
                    }
                    other.val(newValue).trigger('change');
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

            // selects can only have their value
            // set if the corresponding option element exists. 
            var input = $(this),
                name = input.attr("name").substr(18);
            var other = $('select[name="billingAddressVM.' + name + '"]');
            var newValue = input.val();
            if (other.find('option[value="' + newValue + '"]').length === 0) {
                // add and select the option
                var newOption =
                    new Option(
                        input.find('option[value=' + newValue + ']').text(), //text
                        newValue, //value
                        true, //defaultSelected
                        true); //selected
                other.append(newOption);
            }
            other.val(newValue).trigger('change');

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

