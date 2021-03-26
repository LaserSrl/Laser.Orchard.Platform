function buildAddressUI(options) {
    global_CopyingAddresses = (typeof global_CopyingAddresses === 'undefined') ? false : global_CopyingAddresses;
    global_ChangingProgrammatically = (typeof global_ChangingProgrammatically === 'undefined') ? false : global_ChangingProgrammatically;
    global_PreventResetChoices = false;
    var listAddressPrefix = options.elementsPrefix.slice(0, -1); //removes "." 
    var countriesSelect2Options = {
        placeholder: $('#' + options.elementsPrefix + 'CountryId').attr("placeholder")
    };
    var citiesSelect2Options = {
        placeholder: $('#' + options.elementsPrefix + 'CitySelectedId').attr("placeholder"),
        ajax: {
            url: options.cities.getUrl,
            data: function (params) {
                var query = {
                    query: params.term,
                    countryId: $('#' + options.elementsPrefix + 'CountryId option').filter(':selected').val(),
                    isBillingAddress: options.isBillingAddress
                };

                // Query parameters will be ?query=[term]&countryId=123&isBillingAddress=true|false
                return query;
            },
            processResults: function (data) {
                return {
                    results: $.map(data, function (item) {
                        return {
                            id: item.Value,
                            text: item.Label
                        };
                    })

                };
            }
        }
    };
    var provincesSelect2Options = {
        placeholder: $('#' + options.elementsPrefix + 'ProvinceSelectedId').attr("placeholder"),
        ajax: {
            url: options.provinces.getUrl,
            data: function (params) {
                var query = {
                    query: params.term,
                    countryId: $('#' + options.elementsPrefix + 'CountryId option').filter(':selected').val(),
                    cityId: $('#' + options.elementsPrefix + 'CityId').val(),
                    cityName: "",
                    isBillingAddress: options.isBillingAddress
                };

                // Query parameters will be ?query=[term]&countryId=123&isBillingAddress=true|false
                return query;
            },
            processResults: function (data) {
                return {
                    results: $.map(data.Provinces, function (item) {
                        return {
                            id: item.Value,
                            text: item.Text
                        };
                    })

                };
            }
        }
    };
    // merge defaults with additional options
    var countriesOptions = $.extend(countriesSelect2Options, options.countries.select2Options);
    var citiesOptions = $.extend(citiesSelect2Options, options.cities.select2Options);
    var provincesOptions = $.extend(provincesSelect2Options, options.provinces.select2Options);

    $('#' + options.elementsPrefix + 'CountryId').select2(
        countriesOptions
    );
    $('#' + options.elementsPrefix + 'CitySelectedId').select2(
        citiesOptions
    );
    $('#' + options.elementsPrefix + 'ProvinceSelectedId').select2(
        provincesOptions
    );

    //Select 2 Events START
    //On Select 2 City Change
    $('#' + options.elementsPrefix + 'CitySelectedId').on('change', function (e) {
        global_ChangingProgrammatically = true;
        if (!global_CopyingAddresses) {
            if (e.currentTarget.selectedOptions.length > 0) {
                $('#' + options.elementsPrefix + 'CityId').val(e.currentTarget.selectedOptions[0].value);
                $('#' + options.elementsPrefix + 'City').val(e.currentTarget.selectedOptions[0].text);
            } else {
                $('#' + options.elementsPrefix + 'CityId').val(0);
                $('#' + options.elementsPrefix + 'City').val("");
            }
            $('#' + options.elementsPrefix + 'ProvinceId').val(0);
            $('#' + options.elementsPrefix + 'Province').val("");
            $('#' + options.elementsPrefix + 'ProvinceSelectedId').empty();
        }
        global_ChangingProgrammatically = false;
    });
    //On City Change
    $('#' + options.elementsPrefix + 'City').on('change', function (e) {
        if (!global_CopyingAddresses && !global_ChangingProgrammatically) {
            $('#' + options.elementsPrefix + 'CityId').val(0);
        }
    });
    //On Select 2 Province Change
    $('#' + options.elementsPrefix + 'ProvinceSelectedId').on('change', function (e) {
        global_ChangingProgrammatically = true;
        if (!global_CopyingAddresses) {
            if (e.currentTarget.selectedOptions.length > 0) {
                $('#' + options.elementsPrefix + 'ProvinceId').val(e.currentTarget.selectedOptions[0].value);
                $('#' + options.elementsPrefix + 'Province').val(e.currentTarget.selectedOptions[0].text);
            } else {
                $('#' + options.elementsPrefix + 'ProvinceId').val(0);
                $('#' + options.elementsPrefix + 'Province').val("");
            }
        }
        global_ChangingProgrammatically = false;
    });
    //On Province Change
    $('#' + options.elementsPrefix + 'Province').on('change', function (e) {
        if (!global_CopyingAddresses && !global_ChangingProgrammatically) {
            $('#' + options.elementsPrefix + 'ProvinceId').val(0);
        }
    });

    //On Country Change
    $('#' + options.elementsPrefix + 'CountryId').on('change', function (e) {
        if (e.currentTarget.selectedOptions.length > 0) {
            var ajaxParams = {
                territoryId: e.currentTarget.selectedOptions[0].value
            };
            var shouldResetAddressesOnAjaxSuccess = !global_CopyingAddresses && !global_PreventResetChoices;
            $.ajax({
                url: options.countries.administrativeInfoUrl,
                data: ajaxParams,
                success: function (result) {
                    var hasCities = result.HasCities;
                    var hasProvinces = result.HasProvinces;

                    //reset choices
                    if (shouldResetAddressesOnAjaxSuccess) {
                        $('#' + options.elementsPrefix + 'CityId').val(0);
                        $('#' + options.elementsPrefix + 'CitySelectedId').empty();
                        $('#' + options.elementsPrefix + 'City').val("");
                        $('#' + options.elementsPrefix + 'ProvinceId').val(0);
                        $('#' + options.elementsPrefix + 'ProvinceSelectedId').empty();
                        $('#' + options.elementsPrefix + 'Province').val("");
                    }
                    if (hasCities) {
                        Select2ShippingAddressVisibility($('#' + options.elementsPrefix + 'CitySelectedId'), true);
                    }
                    else {
                        Select2ShippingAddressVisibility($('#' + options.elementsPrefix + 'CitySelectedId'), false);
                    }
                    if (hasProvinces) {
                        Select2ShippingAddressVisibility($('#' + options.elementsPrefix + 'ProvinceSelectedId'), true);

                    } else {
                        Select2ShippingAddressVisibility($('#' + options.elementsPrefix + 'ProvinceSelectedId'), false);
                    }
                }
            });
        }
    });
    //Select 2 Events END

    // when a different address is selected:
    $('#' + listAddressPrefix + 'ListAddress').on('change', function (e) {
        arrayOfStoredAddresses = options.arrayOfStoredAddresses;
        if ($(this).val() == -1) {
            $('#' + options.guid).find('input').val('');
            // clear what's currently populated for the address
            global_CopyingAddresses = true;
            $('#' + options.elementsPrefix + 'Honorific').val("").trigger("change");
            $('#' + options.elementsPrefix + 'FirstName').val("").trigger("change");
            $('#' + options.elementsPrefix + 'LastName').val("").trigger("change");
            $('#' + options.elementsPrefix + 'Company').val("").trigger("change");
            $('#' + options.elementsPrefix + 'Address1').val("").trigger("change");
            $('#' + options.elementsPrefix + 'Address2').val("").trigger("change");
            $('#' + options.elementsPrefix + 'PostalCode').val("").trigger("change");
            $('#' + options.elementsPrefix + 'CityId').val(0).trigger("change");
            $('#' + options.elementsPrefix + 'ProvinceId').val(0).trigger("change");
            $('#' + options.elementsPrefix + 'CitySelectedId').empty().trigger("change");
            $('#' + options.elementsPrefix + 'ProvinceSelectedId').empty().trigger("change");
            $('#' + options.elementsPrefix + 'City').val("").trigger("change");
            $('#' + options.elementsPrefix + 'Province').val("").trigger("change");
            global_CopyingAddresses = false;
        } else {
            // figure out what address is selected
            for (i = 0; i < arrayOfStoredAddresses.length; i++) {
                if (arrayOfStoredAddresses[i].Id == $(this).val()) {
                    // set the fields
                    $('#' + options.elementsPrefix + 'Honorific').val(arrayOfStoredAddresses[i].Honorific).trigger("change");
                    $('#' + options.elementsPrefix + 'FirstName').val(arrayOfStoredAddresses[i].FirstName).trigger("change");
                    $('#' + options.elementsPrefix + 'LastName').val(arrayOfStoredAddresses[i].LastName).trigger("change");
                    $('#' + options.elementsPrefix + 'Company').val(arrayOfStoredAddresses[i].Company).trigger("change");
                    $('#' + options.elementsPrefix + 'Address1').val(arrayOfStoredAddresses[i].Address1).trigger("change");
                    $('#' + options.elementsPrefix + 'Address2').val(arrayOfStoredAddresses[i].Address2).trigger("change");
                    $('#' + options.elementsPrefix + 'PostalCode').val(arrayOfStoredAddresses[i].PostalCode).trigger("change");
                    $('#' + options.elementsPrefix + 'CityId').val(arrayOfStoredAddresses[i].CityId).trigger("change");
                    $('#' + options.elementsPrefix + 'ProvinceId').val(arrayOfStoredAddresses[i].ProvinceId).trigger("change");
                    $('#' + options.elementsPrefix + 'City').val(arrayOfStoredAddresses[i].City).trigger("change");
                    $('#' + options.elementsPrefix + 'Province').val(arrayOfStoredAddresses[i].Province).trigger("change");
                    // Set the value, creating a new option if necessary
                    var newOption;
                    if ($('#' + options.elementsPrefix + 'CitySelectedId').find("option[value='" + arrayOfStoredAddresses[i].CityId + "']").length) {
                        $('#' + options.elementsPrefix + 'CitySelectedId').val(arrayOfStoredAddresses[i].CityId).trigger('change');
                    } else {
                        // Create a DOM Option and pre-select by default
                        newOption = new Option(arrayOfStoredAddresses[i].City, arrayOfStoredAddresses[i].CityId, true, true);
                        // Append it to the select
                        $('#' + options.elementsPrefix + 'CitySelectedId').append(newOption).trigger('change');
                    }
                    if ($('#' + options.elementsPrefix + 'ProvinceSelectedId').find("option[value='" + arrayOfStoredAddresses[i].ProvinceId + "']").length) {
                        $('#' + options.elementsPrefix + 'ProvinceSelectedId').val(arrayOfStoredAddresses[i].ProvinceId).trigger('change');
                    } else {
                        // Create a DOM Option and pre-select by default
                        newOption = new Option(arrayOfStoredAddresses[i].Province, arrayOfStoredAddresses[i].ProvinceId, true, true);
                        // Append it to the select
                        $('#' + options.elementsPrefix + 'ProvinceSelectedId').append(newOption).trigger('change');
                    }
                    $('#' + options.elementsPrefix + 'CountryId option[value=' + arrayOfStoredAddresses[i].CountryId + ']').prop('selected', true);
                    global_CopyingAddresses = true;
                    $('#' + options.elementsPrefix + 'CountryId').trigger("change");
                    global_CopyingAddresses = false;
                }
            }
        }
    });

    if (parseInt($('#' + options.elementsPrefix + 'CountryId').val()) > 0) {
        global_PreventResetChoices = true;
        $('#' + options.elementsPrefix + 'CountryId').trigger("change");
        global_PreventResetChoices = false;
    }

    //BackEnd specific handlers 
    if (options.basicAddress != null) {
        $(options.basicAddress.selector).hide();
        var syncElements = [{
            source: "Honorific", target: "Honorific"
        }, {
            source: "FirstName", target: "FirstName"
        }, {
            source: "LastName", target: "LastName"
        }, {
            source: "Company", target: "Company"
        }, {
            source: "PostalCode", target: "PostalCode"
        }, {
            source: "Address1", target: "Address1"
        }, {
            source: "Address2", target: "Address2"
        }, {
            source: "City", target: "City"
        }, {
            source: "Province", target: "Province"
        }, {
            source: "CitySelectedId", target: "City"
        }, {
            source: "ProvinceSelectedId", target: "Province"
        }, {
            source: "CountryId", target: "Country"
        }];
        syncElements.forEach(function (item, index) {
            $('#' + options.elementsPrefix + item.source).on('change', function (e) {
                if ($(this).prop("tagName").toLowerCase() == "input") {
                    $('#' + options.basicAddress.elementsPrefix + item.target).val($(this).val());
                } else if ($(this).prop("tagName").toLowerCase() == "select") {
                    if ($('option:selected', this) != null) {
                        $('#' + options.basicAddress.elementsPrefix + item.target).val($('option:selected', this).text());
                    } else {
                        $('#' + options.basicAddress.elementsPrefix + item.target).val('');
                    }
                }
            });

        });
    }


    // after creation of all Select2 UI we ensure visibility
    EnsureVisibility(options);

}

function Select2ShippingAddressVisibility(select2Element, show) {
    var textElement = $('#' + select2Element.attr('id').slice(0, -10));
    if (show) {
        textElement.hide();
        select2Element.next(".select2-container").show();
    } else {
        select2Element.next(".select2-container").hide();
        textElement.show();
    }
}

function EnsureVisibility(options) {
    //Ensure visibility of select2/Text inputs based on their Id values
    if (options.cities.selectedId > 0) {
        Select2ShippingAddressVisibility($('#' + options.elementsPrefix + 'CitySelectedId'), true);
    } else {
        Select2ShippingAddressVisibility($('#' + options.elementsPrefix + 'CitySelectedId'), false);
    }
    if (options.provinces.selectedId > 0) {
        Select2ShippingAddressVisibility($('#' + options.elementsPrefix + 'ProvinceSelectedId'), true);
    } else {
        Select2ShippingAddressVisibility($('#' + options.elementsPrefix + 'ProvinceSelectedId'), false);
    }
}

