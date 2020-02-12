/* *
 * Private variables
 * */
var niAC, // instance of the object for address configuration
    AddressConfiguration = function () { },
    _isJQ = !!(window.jQuery),
    _window = $(window);
/* *
 * Private functions
 * */
var _checkInstance = function () {
    if (!$.addressConfiguration.instance) {
        niAC = new AddressConfiguration();
        niAC.init();
        $.addressConfiguration.instance = niAC;
    }
};
/* *
 * Public functions
 * */
AddressConfiguration.prototype = {
    constructor: AddressConfiguration,
    init: function () { },
    /* *
     * Initialize the configuration scripts for an address
     * */
    addAddress: function (el, options) {
        // disable all input within the address div/form
        // wherever that input has the class that marks it as belonging
        // to the address and being affected by the script
        el.find(options.inputSelector).prop("disabled", true);
        // some stuff is always free text
        el.find(options.freeInputSelector).prop("disabled", false);
        // set some variables as shorthands for the various inputs
        var countryInput = el.find(options.countriesInput);
        // attach handlers
        // When selected country changes...
        niAC._attachCountryHandlers(el, options);
        // When selected city changes...
        niAC._attachCityHandlers(el, options);
        // When selected province changes...
        niAC._attachProvinceHandlers(el, options);
        // Re-enable input for Country.
        countryInput.prop("disabled", false);
        // Re-enable inputs that should be free-text all the time
        niAC._enableCityInput(el, options);
        niAC._enableProvinceInput(el, options);
        // If input for Country already has a valid (> 0) value, enable
        // input for City (really, fire handlers)
        if (niAC._checkCountryOption(el, options)) {
            countryInput.trigger("change");
        }
    },
    /* *
     * Reset the address form 
     * */
    reset: function (el, options, values) {
        el.find('input').val('');
        // reinit
        var countryInput = el.find(options.countriesInput);
        var nullOption = countryInput
            .find('option')
            .filter(function () {
                return $(this).attr('value') <= 0;
            });
        nullOption.prop("disabled", false);
        countryInput.val(nullOption.attr('value'));
        // set values
        if (values) {
            // set some variables as shorthands for the various inputs
            var citiesInput = el.find(options.citiesInput);
            var cityIdInput = el.find(options.cityId);
            var provincesInput = el.find(options.provincesInput);
            var provinceIdInput = el.find(options.provinceId);
            // province
            provinceIdInput.val(values.provinceId);
            if (provincesInput.is('select')) {
                provincesInput.val(values.provinceId);
            } else {
                provincesInput.val(values.province);
            }
            // city
            cityIdInput.val(values.cityId);
            if (citiesInput.is('select')) {
                citiesInput.val(values.cityId);
            } else {
                citiesInput.val(values.city);
            }
            // country
            countryInput.val(values.countryId);

            //niAC._enableCityInput(el, options);
            //niAC._enableProvinceInput(el, options);
        }
        niAC._detachCountryHandlers(el, options);
        niAC._detachCityHandlers(el, options);
        niAC._detachProvinceHandlers(el, options);
        niAC.addAddress(el, options);
        //if (niAC._checkCountryOption(el, options)) {
        //    countryInput.trigger("change");
        //}
    },
    /* *
     * Helpers/Handlers
     * */
    _baseCountryChangeHandler: function (e) {
        // countries with value < 0 are just placeholders
        // $('#CountryId').find('option').filter(function() {return $(this).attr('value') <= 0})
        e.data.el
            .find(e.data.options.countriesInput)
            .find('option')
            .filter(function () {
                return $(this).attr('value') <= 0;
            })
            .prop("disabled", true);
        niAC._enableCityInput(e.data.el, e.data.options);
    },
    _baseCityChangeHandler: function (e) {
        niAC._enableProvinceInput(e.data.el, e.data.options);
    },
    _baseProvinceChangeHandler: function (e) {

    },
    // attach country handlers
    _attachCountryHandlers: function (el, options) {
        var countryInput = el.find(options.countriesInput);
        if (options.useDefaultCountryChangeHandler) {
            countryInput.on('change', {el:el, options:options}, niAC._baseCountryChangeHandler);
        }
        if (options.onChangeCountry) {
            countryInput.on('change', options.onChangeCountry);
        }
    },
    // attach city handlers
    _attachCityHandlers: function (el, options) {
        var cityInput = el.find(options.citiesInput);
        if (cityInput.is('select')) {
            cityInput.on('change', function (e) {
                el.find(options.cityId).val(cityInput.val());
            });
        }
        if (options.useDefaultCityChangeHandler) {
            cityInput.on('change', { el: el, options: options }, niAC._baseCityChangeHandler);
        }
        if (options.onChangeCity) {
            cityInput.on('change', options.onChangeCity);
        }
    },
    // attach province handlers
    _attachProvinceHandlers: function (el, options) {
        var provinceInput = el.find(options.provincesInput);
        if (provinceInput.is('select')) {
            provinceInput.on('change', function (e) {
                el.find(options.provinceId).val(provinceInput.val());
            });
        }
        if (options.useDefaultProvinceChangeHandler) {
            provinceInput.on('change', {el:el, options:options}, niAC._baseProvinceChangeHandler);
        }
        if (options.onChangeProvince) {
            provinceInput.on('change', options.onChangeProvince);
        }
    },
    // detach country handlers
    _detachCountryHandlers: function (el, options) {
        var countryInput = el.find(options.countriesInput);
        if (options.useDefaultCountryChangeHandler) {
            countryInput.off('change', niAC._baseCountryChangeHandler);
        }
        if (options.onChangeCountry) {
            countryInput.off('change', options.onChangeCountry);
        }
    },
    // detach city handlers
    _detachCityHandlers: function (el, options) {
        var cityInput = el.find(options.citiesInput);
        if (options.useDefaultCityChangeHandler) {
            cityInput.off('change', niAC._baseCityChangeHandler);
        }
        if (options.onChangeCity) {
            cityInput.off('change', options.onChangeCity);
        }
    },
    // detach province handlers
    _detachProvinceHandlers: function (el, options) {
        var provinceInput = el.find(options.provincesInput);
        if (options.useDefaultProvinceChangeHandler) {
            provinceInput.off('change', niAC._baseProvinceChangeHandler);
        }
        if (options.onChangeProvince) {
            provinceInput.off('change', options.onChangeProvince);
        }
    },
    // check that the country field exists and its value is valid
    _checkCountryOption: function (el, options) {
        var countryInput = el.find(options.countriesInput);
        if (countryInput.val() && countryInput.val() > 0) {
            return true;
        }
        return false;
    },
    // enable and populate city input
    _enableCityInput: function (el, options) {
        if (!!options.cityIsFreeText) {
            this._enableAllCity(el, options, true)
        } else {
            if (this._checkCountryOption(el, options)) {
                var viewModel = {
                    CountryId:  el.find(options.countriesInput).val()
                };
                $.post(options.getCities.url, {
                    viewmodel: viewModel,
                    __RequestVerificationToken: options.token
                })
                    .done(function (data) {
                        // We are here when the call returns and OK HTTP code
                        // it still may not be successfull, so we have to check 
                        // in the received data
                        if (data.Success) {
                            // city's input
                            var city = el.find(options.citiesInput);
                            // detach handlers
                            niAC._detachCityHandlers(el, options);
                            // call delegates?
                            if (options.getCities.before) {
                                options.getCities.before(city);
                            }
                            // populate the choices for the city
                            if (data.Cities && data.Cities.length > 0) {
                                // multiple cities => dropdown
                                var newInput = '<select ';
                                for (var a = 0; a < city[0].attributes.length; a++) {
                                    newInput += city[0].attributes[a].name + '="' + city[0].attributes[a].value + '" ';
                                }
                                newInput += '></select>';
                                city.replaceWith(newInput);
                                city = el.find(options.citiesInput);
                                var cityId = el.find(options.cityId).val();
                                // remove old values
                                //city.empty();
                                for (var i = 0; i < data.Cities.length; i++) {
                                    // create a DOM option
                                    var newOption =
                                        new Option(
                                            data.Cities[i].Text, //text
                                            data.Cities[i].Value, //value
                                            !!data.Cities[i].DefaultSelected, //defaultSelected
                                            !!data.Cities[i].Selected
                                                || data.Cities[i].Value == cityId); //selected
                                    city.append(newOption);
                                }
                            } else {
                                // no configured city => free text
                                var newInput = '<input type="text" ';
                                for (var a = 0; a < city[0].attributes.length; a++) {
                                    newInput += city[0].attributes[a].name + '="' + city[0].attributes[a].value + '" ';
                                }
                                newInput += '/>';
                                city.replaceWith(newInput);
                                city = el.find(options.citiesInput);
                            }
                            // call delegates?
                            if (options.getCities.after) {
                                options.getCities.after(city);
                            }
                            // reattach handlers
                            niAC._attachCityHandlers(el, options);
                            // enable the city's input
                            niAC._enableAllCity(el, options, true)
                            // trigger change event
                            city.trigger("change");
                        } else {
                            // notify?
                            // retry?
                        }
                    })
                    .fail(function () {
                        // we are here when we receive an HTTP error code
                    })
                    .always(function () { });
            }
        }
    },
    _enableAllCity: function (el, options, status) {
        // status is the status we want to reach:
        // true => we want to enable => we set disabled to false
        // false => we want to disable => we set disabed to true
        el.find(options.citiesInput).prop("disabled", !!!status);
        el.find(options.withCitySelector).prop("disabled", !!!status);
    },
    // enable and populate province input
    _enableProvinceInput: function (el, options) {
        if (!!options.provinceIsFreeText) {
            this._enableAllProvince(el, options, true)
        } else {
            var cityId = 0;
            var cityName = "";
            var city = el.find(options.citiesInput);
            if (city.is('select')) {
                cityId = city.val();
            } else {
                cityName = city.val();
            }
            if (!cityId) {
                cityId = el.find(options.cityId).val();
            }

            if (!cityId) {
                cityId = 0; // handle case where the select is unset
            }
            var viewModel = {
                CountryId: el.find(options.countriesInput).val(),
                CityId: cityId,
                CityName: cityName //in case city is input rather than select
            };
            $.post(options.getProvinces.url, {
                viewmodel: viewModel,
                __RequestVerificationToken: options.token
            })
                .done(function (data) {
					// We are here when the call returns and OK HTTP code
					// it still may not be successfull, so we have to check 
                    // in the received data
                    if (data.Success) {
                        // province input
                        var province = el.find(options.provincesInput);
                        // detach handlers
                        niAC._detachProvinceHandlers(el, options);
                        // call delegates?
                        if (options.getProvinces.before) {
                            options.getProvinces.before(province);
                        }
                        if (data.Provinces && data.Provinces.length > 0) {
                            // dropdown
                            var newInput = '<select ';
                            for (var a = 0; a < province[0].attributes.length; a++) {
                                newInput += province[0].attributes[a].name + '="' + province[0].attributes[a].value + '" ';
                            }
                            newInput += '></select>';
                            province.replaceWith(newInput);
                            province = el.find(options.provincesInput);
                            var provinceId = el.find(options.provinceId);
                            // remove old values
                            //province.empty();
                            for (var i = 0; i < data.Provinces.length; i++) {
                                // create a DOM option
                                var newOption =
                                    new Option(
                                        data.Provinces[i].Text, //text
                                        data.Provinces[i].Value, //value
                                        !!data.Provinces[i].DefaultSelected, //defaultSelected
                                        !!data.Provinces[i].Selected
                                            || data.Provinces[i].Value == provinceId); //selected
                                province.append(newOption);
                            }
                        } else {
                            // no configured province => free text
                            var newInput = '<input type="text" ';
                            for (var a = 0; a < province[0].attributes.length; a++) {
                                newInput += province[0].attributes[a].name + '="' + province[0].attributes[a].value + '" ';
                            }
                            newInput += '/>';
                            province.replaceWith(newInput);
                            province = el.find(options.provincesInput);
                        }
                        // call delegates?
                        if (options.getProvinces.after) {
                            options.getProvinces.after(province);
                        }
                        // reattach handlers
                        niAC._attachProvinceHandlers(el, options);
                        // enable inputs
                        niAC._enableAllProvince(el, options, true);
                        // trigger change event
                        province.trigger("change");
                    } else {
						// notify?
						// retry?
                    }
                })
                .fail(function () {
                    // we are here when we receive an HTTP error code
                })
                .always(function () { });
        }
    },
    _enableAllProvince: function (el, options, status) {
        // status is the status we want to reach:
        // true => we want to enable => we set disabled to false
        // false => we want to disable => we set disabed to true
        el.find(options.provincesInput).prop("disabled", !!!status);
        el.find(options.withProvinceSelector).prop("disabled", !!!status);
    },

};
/* *
 * Public static functions 
 * */
$.addressConfiguration = {
    instance: null,
    proto: AddressConfiguration.prototype,

    defaults: {
        // default option values
        countriesInput: '', // CSS/jQuery selector of input for the country
        citiesInput: '', // CSS/jQuery selector of input for the city
        cityId: '', // CSS/jQuery selector of input for the city id
        provincesInput: '', // CSS/jQuery selector of input for the province
        provinceId: '', // CSS/jQuery selector of input for the province id
        inputSelector: '.address-input', //CSS/jQuery selector of all address input
        /* *
         * Flags for special behaviours
         * */
        countryIsFreeText: false,
        cityIsFreeText: false,
        provinceIsFreeText: false,
        /* *
         * Stuff from this selector is considered free input and not handled specifically
         * */
        freeInputSelector: '.free-text',
        /* *
         * Stuff from this selector has the same state as the city input
         * */
        withCitySelector: '.configure-with-city',
        /* *
         * Stuff from this selector has the same state as the province input
         * */
        withProvinceSelector: '.configure-with-province',
        /* *
         * Should we use some default event handlers for changes of selection for city
         * and country? (these are not overridden by adding custom handlers)
         * */
        useDefaultCountryChangeHandler: true,
        useDefaultCityChangeHandler: true,
        useDefaultProvinceChangeHandler: true,
        /* *
         * Use selected city to limit choice of province
         * */
        cityCommandsProvince: true,
    }
};
$.fn.addressConfiguration = function (options) {
    _checkInstance();
    var containerDiv = $(this);

    options = $.extend(true, {}, $.addressConfiguration.defaults, options);
    options.token = containerDiv
        .closest("form")
        .find("input[name='__RequestVerificationToken']")
        .val();

    /* *
     * VALIDATION
     * */
    if (!options.token) {
        console.error('RequestVerificationToken is required.');
        return;
    }
    if (!options.getCities.url) {
        console.error('Url to get cities is required.');
        return;
    }
    if (!options.getProvinces.url) {
        console.error('Url to get provinces is required.');
        return;
    }

    // set options as attributes for the html element
    containerDiv.attr("niac-options", JSON.stringify(options));

    niAC.addAddress(containerDiv, options);

    return containerDiv;
};
$.fn.resetAddress = function (values, options) {
    _checkInstance();
    var containerDiv = $(this);

    if (!options) {
        if (containerDiv.attr("niac-options")) {
            options = JSON.parse(containerDiv.attr("niac-options"));
        }
    }
    options = $.extend(true, {}, $.addressConfiguration.defaults, options);
    options.token = containerDiv
        .closest("form")
        .find("input[name='__RequestVerificationToken']")
        .val();
    /* *
     * VALIDATION
     * */
    if (!options.token) {
        console.error('RequestVerificationToken is required.');
        return;
    }
    if (!options.getCities.url) {
        console.error('Url to get cities is required.');
        return;
    }
    if (!options.getProvinces.url) {
        console.error('Url to get provinces is required.');
        return;
    }

    niAC.reset(containerDiv, options, values);

    return containerDiv;
};
