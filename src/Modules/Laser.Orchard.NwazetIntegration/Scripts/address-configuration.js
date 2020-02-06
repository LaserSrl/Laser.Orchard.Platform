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
        var countryChangeHandler = function (e) {
            niAC._enableCityInput(el, options);
        };

        // disable all input within the address div/form
        // wherever that input has the class that marks it as belonging
        // to the address and being affected by the script
        el.find(options.inputSelector).prop("disabled", true);
        // some stuff is always free text
        el.find(options.freeInputSelector).prop("disabled", false);
        // set some variables as shorthands for the various inputs
        var countryInput = el.find(options.countriesInput);
        var cityInput = el.find(options.citiesInput);
        var provinceInput = el.find(options.provincesInput);
        // attach handlers
        // When selected country changes...
        el.on('change', options.countriesInput, countryChangeHandler);
        // Re-enable input for Country.
        countryInput.prop("disabled", false);
        // Re-enable inputs that should be free-text all the time
        niAC._enableCityInput(el, options);
        niAC._enableProvinceInput(el, options);
        // If input for Country already has a valid (> 0) value, enable
        // input for City
        if (niAC._checkCountryOption(el, options)) {
            niAC._enableCityInput(el, options);
        }
    },
    /* *
     * Helpers/Handlers
     * */
    _checkCountryOption: function (el, options) {
        var countryInput = el.find(options.countriesInput);
        if (countryInput.val() && countryInput.val() > 0) {
            return true;
        }
        return false;
    },
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
                            // populate the choices for the city
                            if (data.Cities && data.Cities.length > 0) {
                                for (var i = 0; i < data.Cities.length; i++) {
                                    // create a DOM option
                                    var newOption =
                                        new Option(data.Cities[i].Text, data.Cities[i].Value, false, false);
                                    el.find(options.citiesInput).append(newOption);
                                }
                            }
                            // call delegates?
                            if (options.getCities.delegate) {
                                options.getCities.delegate();
                            }
                            // enable the city's input
                            this._enableAllCity(el, options, true)
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
    _enableProvinceInput: function (el, options) {
        if (!!options.provinceIsFreeText) {
            this._enableAllProvince(el, options, true)
        } else {
            // TODO: ajax call to list of valid provinces
            // onSuccess of that call populate the information
            // then enable the input
            this._enableAllProvince(el, options, true)
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
        provincesInput: '', // CSS/jQuery selector of input for the province
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

 //   /*
 //    * //I COPIED THIS FROM MAGNIFIC POPUP AND I AM NOT SURE ABOUT IT
	//* As Zepto doesn't support .data() method for objects
	//* and it works only in normal browsers
	//* we assign "options" object directly to the DOM element. FTW!
	//*/
 //   if (_isJQ) {
 //       containerDiv.data('addressConfiguration', options);
 //   } else {
 //       containerDiv[0].addressConfiguration = options;
 //   }


    niAC.addAddress(containerDiv, options);

    return containerDiv;
};