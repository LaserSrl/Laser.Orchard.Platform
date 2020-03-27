/**
 * Copyright (C) 2012 Chris Wharton (chris@weare2ndfloor.com)
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * THIS SOFTWARE AND DOCUMENTATION IS PROVIDED "AS IS," AND COPYRIGHT
 * HOLDERS MAKE NO REPRESENTATIONS OR WARRANTIES, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO, WARRANTIES OF MERCHANTABILITY OR
 * FITNESS FOR ANY PARTICULAR PURPOSE OR THAT THE USE OF THE SOFTWARE
 * OR DOCUMENTATION WILL NOT INFRINGE ANY THIRD PARTY PATENTS,
 * COPYRIGHTS, TRADEMARKS OR OTHER RIGHTS.COPYRIGHT HOLDERS WILL NOT
 * BE LIABLE FOR ANY DIRECT, INDIRECT, SPECIAL OR CONSEQUENTIAL
 * DAMAGES ARISING OUT OF ANY USE OF THE SOFTWARE OR DOCUMENTATION.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://gnu.org/licenses/>.
 
 Documentation available at http://cookiecuttr.com
 
 Modified by Laser.
 */
(function ($) {
    function load() {
        $.cookieCuttr = function (pOptions) {
			var defaults = {
				cookieBannerPosition: "top",
                cookieCutter: false, // you'd like to enable the div/section/span etc. hide feature? change this to true
                cookieAcceptButton: true, // this will disable non essential cookies
                cookieResetButton: false,
				cookiePolicyLink: '/privacy-policy/', // if applicable, enter the link to your privacy policy here...
				cookieTitle: "This website uses cookies",
                cookieMessage: 'We use cookies on this website, you can <a href="{{cookiePolicyLink}}" title="read about our cookies">read about them here</a>. To use the website as intended please...',
                cookieWhatAreTheyLink: "http://www.allaboutcookies.org/",
                cookieDisable: '',
                cookieExpires: 365,
                cookieAcceptButtonText: "ACCEPT COOKIES",
                cookieResetButtonText: "RESET COOKIES FOR THIS WEBSITE",
                cookieWhatAreLinkText: "What are cookies?",
                cookiePolicyPage: false,
                cookiePolicyPageMessage: 'Please read the information below and then choose from the following options',
                cookieDiscreetReset: false,
                cookieNoMessage: false, // change to true hide message from all pages apart from your policy page
                cookieDomain: "",
                cookieExpandMessage: "Change your choice",
				cookiePoweredBy: "",
				cookiePoweredByIcon: ""
            };
            var options = $.extend(defaults, pOptions);
            //convert options
			var cookieBannerPosition = options.cookieBannerPosition;
			var cookieTitle = options.cookieTitle;
			var cookieMessage = options.cookieMessage;
            var cookieExpires = options.cookieExpires;
            var cookieAcceptButtonText = options.cookieAcceptButtonText;
            var cookiePolicyPage = options.cookiePolicyPage;
            var cookieExpectedValue = options.cookieExpectedValue;
            var cookieAccepted = options.cookieAccepted;
            var cookieExpandMessage = options.cookieExpandMessage;
			var cookiePoweredBy = options.cookiePoweredBy;
			var cookiePoweredByIcon = options.cookiePoweredByIcon;
            // cookie identifier

            var manageCookieResetButton = function (options) {
                // write cookie reset button
				if (options.cookieResetButton) {
					var htmlDiscreetReset = '<div class="cc-cookies-reset cc-discreet"><a class="cc-cookie-reset" href="#" title="' + options.cookieResetButtonText + '">' + options.cookieResetButtonText + '</a></div>';
					var htmlReset = '<div class="cc-cookies-reset"><a class="cc-cookie-reset" href="#" title="' + options.cookieResetButtonText + '">' + options.cookieResetButtonText + '</a></div>';
					if (options.cookieDiscreetReset) {
						if (cookieBannerPosition == "top") {
							$('body').prepend(htmlDiscreetReset);
							$('div.cc-cookies-reset.cc-discreet').css("top", "0")
							$('div.cc-cookies-reset.cc-discreet').css("border-bottom-left-radius", "5px")
							$('div.cc-cookies-reset.cc-discreet').css("border-bottom-right-radius", "5px")
						} else { // bottom, overlay
							$('body').append(htmlDiscreetReset);
							$('div.cc-cookies-reset.cc-discreet').css("bottom", "0")
							$('div.cc-cookies-reset.cc-discreet').css("border-top-left-radius", "5px")
							$('div.cc-cookies-reset.cc-discreet').css("border-top-right-radius", "5px")
						}
					} else {
						if (cookieBannerPosition == "top") {
							$('body').prepend(htmlReset);
						} else {
							$('body').append(htmlReset);
						}
					}
				}
                //reset cookies
                $('a.cc-cookie-reset').click(function (f) {
                    f.preventDefault();
                    // fire an event to notify we are resetting cookie consent
                    $(document).trigger("cookieConsent.reset");
                    $.cookie("cc_cookie_accept", null, {
                        path: '/'
                    });
                    $(".cc-cookies-reset").fadeOut(function () {
                        // reload page to activate cookies
                        location.reload();
                    });
                });
                $('div.cc-cookies-reset').show();
            }

            var $cookieAccepted = cookieAccepted;
            $.cookieAccepted = function () {
                return $cookieAccepted;
            };
            // layout of cookie accept button
			var cookieAccept = ' <a href="#accept" class="cc-cookie-accept">' + cookieAcceptButtonText + '</a> ';
            if ($cookieAccepted) {
                manageCookieResetButton(options);
            } else {
                // add message to just after opening body tag
                var htmlFooterText = 'GDPR Cookies';
                if (cookieExpandMessage != "") {
                    htmlFooterText = '<a class="cc-expand">' + cookieExpandMessage + '</a> | ' + htmlFooterText;
                }
                var iconPoweredBy = '|';
                var htmlBanner = '<div class="cc-cookies" style="background-color:black;box-shadow:#121212 2px 2px 14px 2px"><div class="cc-cookie-box" style="width:90%;max-width:670px;margin:0px auto;width:auto;padding:0px;"><h3>' + cookieTitle + "</h3>" + cookieMessage + '<div style="position:relative"><div id="ccPoweredBy" style="height:34px"><div style="position:absolute;bottom:0px;margin-bottom:6px"><p id="ccPoweredBy">' + htmlFooterText + ' ' + iconPoweredBy + ' ' + cookiePoweredBy + '</p></div></div><div style="position:absolute;right:0;bottom:0">' + cookieAccept + '</div></div></div></div>';
                var htmlOverlay = '<div class="cc-cookies cc-overlay"><div class="cc-cookie-frame"><div class="cc-cookie-box"><h3>' + cookieTitle + "</h3><p>" + cookieMessage + "</p>" + cookieAccept + '<hr/><p id="ccPoweredBy">' + htmlFooterText + ' ' + iconPoweredBy + ' ' + cookiePoweredBy + '</p></div></div></div>';
				if (cookiePolicyPage) {
					if (cookieBannerPosition == "top") {
						$('body').prepend(htmlBanner);
					} else {
						$('body').append(htmlBanner);
					}
				} else if (cookieBannerPosition == "overlay") {
					$('body').append(htmlOverlay);
				} else if (cookieBannerPosition == "top") {
					$('body').prepend(htmlBanner);
                } else { // bottom
					$('body').append(htmlBanner);
                }
			}
			// do not cover policy page
			if (cookiePolicyPage) {
				$('div.cc-cookies').css("position", "relative");
			}
			//add appropriate CSS depending on position chosen
			if (cookieBannerPosition == "top") {
				$('div.cc-cookies').css("top", "0");
			} else { // overlay, bottom
				$('div.cc-cookies').css("bottom", "0");
			}
            // setting the cookies
            $('.cc-cookie-accept').click(function (e) {
                e.preventDefault();

                var acceptedOptions = {
                    preferences: false,
                    statistical: false,
                    marketing: false
                };
                var aux1 = "";
                if ($("#chkPreferences").prop("checked")) {
                    aux1 = aux1 + "1";
                    acceptedOptions.preferences = true;
                } else {
                    aux1 = aux1 + "0";
                }
                if ($("#chkStatistical").prop("checked")) {
                    aux1 = aux1 + "1";
                    acceptedOptions.statistical = true;
                } else {
                    aux1 = aux1 + "0";
                }
                if ($("#chkMarketing").prop("checked")) {
                    aux1 = aux1 + "1";
                    acceptedOptions.marketing = true;
                } else {
                    aux1 = aux1 + "0";
                }
                $.cookie("cc_cookie_accept", cookieExpectedValue + aux1, {
                    expires: cookieExpires,
                    path: '/'
                });
                $(".cc-cookies").fadeOut(function () {
                    // reload page to activate cookies
                    //location.reload();
                    // fire an event to notify of the accepted options
                    $(document).trigger("cookieConsent.accept", acceptedOptions);
                    manageCookieResetButton(options);
                });
            });
            $('.cc-expand').click(function () {
                $('.cc-cookie-box form').slideToggle('slow');
            });
        };
    }
    return load();
})(jQuery);