var consentCookie = {
	/**
	 * @description The method allows to set the cookie with name, value and time to live passed as input.
	 * @method setCookie
	 * @param {} c_name
	 * @param {} value
	 * @param {} exdays
	 * @return
	 */
	setCookie : function (c_name, value, exdays) {
		var exdate = new Date();
		exdate.setDate(exdate.getDate() + exdays);
		var c_value = escape(value) + ((exdays == null) ? "" : "; expires=" + exdate.toUTCString()) + "; Domain=.laser-group.com" + "; path=/";
		document.cookie = c_name + "=" + c_value;
	},

	/**
	 * @description The method allows to retrieve the cookie with name passed as input.
	 * @method getCookie
	 * @param {} cname
	 * @return Literal
	 */
	getCookie : function (cname) {
		var name = cname + "=";
		var ca = document.cookie.split(';');
		for (var i = 0; i < ca.length; i++) {
			var c = jQuery.trim(ca[i]);

			if (c.indexOf(name) == 0)
				return c.substring(name.length, c.length);
		}
		return "";
	},

	setConsentCookieIfNotExist : function (consentCookieValue) {
		if (consentCookie.getCookie("consentCookie") == "") {
			consentCookie.setCookie("consentCookie", consentCookieValue, 365);
		}
	}
};