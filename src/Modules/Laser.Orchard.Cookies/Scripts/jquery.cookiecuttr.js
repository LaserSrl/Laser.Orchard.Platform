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
                cookieAcceptSelectedButtonText: "ACCEPT SELECTED",
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
            var cookieAcceptSelectedButtonText = options.cookieAcceptSelectedButtonText;
            var cookiePolicyPage = options.cookiePolicyPage;
            var cookieExpectedValue = options.cookieExpectedValue;
            var cookieAccepted = options.cookieAccepted;
            var cookieExpandMessage = options.cookieExpandMessage;
            var cookiePoweredBy = options.cookiePoweredBy;
            var cookiePoweredByIcon = options.cookiePoweredByIcon;
            // cookie identifier

            var $cookieAccepted = cookieAccepted;
            // layout of cookie accept button
            var cookieAccept = ' <a href="#accept" class="cc-cookie-accept">' + cookieAcceptButtonText + '</a> ';
            var cookieAcceptSelected = ' <div class="cc-cookie-accept-selected"><a href="#accept-selected">' + cookieAcceptSelectedButtonText + '</a><div> ';
            if ($cookieAccepted) {
                DrawCookiesResetButton(options);
            } else {
                DrawCookiesBanner(options);
            }


            //var functions
            $.cookieAccepted = function () {
                return $cookieAccepted;
            };

            //functions
            function DrawCookiesResetButton(options) {
                if (!$('div.cc-cookies-reset').length) {
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
                        // resets all cookies except those saved in the list as technical cookies
                        DrawCookiesBanner(options);
                    });
                }
                $('div.cc-cookies').hide(); //hide banner
                $('div.cc-cookies-reset').fadeIn(); //show reset
            }
            function DrawCookiesBanner(options) {
                if (!$('.cc-cookies').length) {
                    // add message to just after opening body tag
                    var htmlFooterText = '';
                    if (cookieExpandMessage != "") {
                        htmlFooterText = '<a class="cc-expand">' + cookieExpandMessage + '</a> ';
                    }
                    var iconPoweredBy = '|';
                    var htmlBanner = '<div class="cc-cookies cc-banner"><div class="cc-cookie-box"><h3>' + cookieTitle + "</h3>" + cookieMessage + '<div class="cc-powered-by-box"><div id="ccPoweredBy"><div class="cc-powered-by-left"><p id="ccPoweredBy">' + htmlFooterText + ' ' + iconPoweredBy + ' ' + cookiePoweredBy + '</p></div></div><div class="cc-powered-by-right">' + cookieAccept + '</div></div></div></div>';
                    var htmlOverlay = '<div class="cc-cookies cc-overlay"><div class="cc-cookie-frame"><div class="cc-cookie-box"><h3>' + cookieTitle + "</h3><p>" + cookieMessage + "</p>" + cookieAccept + '<hr/><p id="ccPoweredBy">' + htmlFooterText + ' ' + iconPoweredBy + ' ' + cookiePoweredBy + '</p></div></div></div>';
                    //drawing banner
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
                    $('.cc-cookie-checks').append(cookieAcceptSelected);
                    // banner events
                    $('.cc-expand').click(function (e) {
                        e.preventDefault();
                        ToggleChoices();
                    });
                    // setting the cookies
                    $('.cc-cookie-accept-selected a').click(function (e) {
                        e.preventDefault();
                        AcceptCookies(false);
                    });
                    $('.cc-cookie-accept').click(function (e) {
                        e.preventDefault();
                        AcceptCookies(true);
                    });
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
                $('div.cc-cookies').fadeIn(); //show banner
                $('div.cc-cookies-reset').hide(); //hide reset
                if ($.cookie('cc_cookie_accept')) {
                    var choices = $.cookie('cc_cookie_accept').substr($.cookie('cc_cookie_accept').length - 3).split('');
                    if (choices[0] == "1") {
                        $("#chkPreferences").prop('checked', true);
                    }
                    if (choices[1] == "1") {
                        $("#chkStatistical").prop('checked', true);
                    }
                    if (choices[2] == "1") {
                        $("#chkMarketing").prop('checked', true);
                    }
                    ExpandChoices();
                }
            }
            function ResetCookies(cookiesToKeep) {
                // removes all cookies that are not in cookiesToKeep list
                var allCookie = $.cookie();
                for (var cookie in allCookie) {
                    // if the cookie is not in the list it will be deleted
                    if (!cookiesToKeep.includes(cookie)) {
                        var defaultDomain = window.DefaultCookieDomain;
                        var genericDeleted = true;
                        // try remove complete defaultDomain
                        if ($.removeCookie(cookie, { path: '/', domain: defaultDomain })) {
                            genericDeleted = false;
                        }
                        if (defaultDomain !== '') {
                            // try remove www. default cookie domain
                            var tryDomain = "www." + defaultDomain;
                            if ($.removeCookie(cookie, { path: '/', domain: tryDomain })) {
                                genericDeleted = false;
                            }
                            // try remove another domain
                            defaultDomain = "." + defaultDomain;
                        }
                        var partCookieDomain = defaultDomain.split('.');
                        for (var i = 0; i < partCookieDomain.length; i++) {
                            if (i !== 0) {
                                // remove another part of domain
                                defaultDomain = defaultDomain.replace('.' + partCookieDomain[i], '');
                            }
                            // excludes .com
                            if (i === partCookieDomain.length - 2) {
                                break;
                            }

                            if ($.removeCookie(cookie, { path: '/', domain: defaultDomain })) {
                                genericDeleted = false;
                                break;
                            }
                        }
                        // if you failed to delete it with any domain
                        // generic deletion
                        if (genericDeleted) {
                            $.removeCookie(cookie, { path: '/' });
                        }
                    }
                }
            }
            function AcceptCookies(acceptAll) {
                var savedCookies = [];
                // technical always selected
                savedCookies = savedCookies.concat(window.TechnicalCookies);


                var acceptedOptions = {
                    preferences: false,
                    statistical: false,
                    marketing: false
                };
                var aux1 = "";
                if (acceptAll) {
                    aux1 = aux1 + "1"; //Preferences
                    acceptedOptions.preferences = true;
                    savedCookies = savedCookies.concat(window.PreferencesCookies);
                    aux1 = aux1 + "1"; //Statistical
                    acceptedOptions.statistical = true;
                    savedCookies = savedCookies.concat(window.StatisticalCookies);
                    aux1 = aux1 + "1"; //Marketing
                    acceptedOptions.marketing = true;
                    savedCookies = savedCookies.concat(window.MarketingCookies);
                }
                else {
                    if ($("#chkPreferences").prop("checked")) {
                        aux1 = aux1 + "1";
                        acceptedOptions.preferences = true;
                        savedCookies = savedCookies.concat(window.PreferencesCookies);
                    } else {
                        aux1 = aux1 + "0";
                    }
                    if ($("#chkStatistical").prop("checked")) {
                        aux1 = aux1 + "1";
                        acceptedOptions.statistical = true;
                        savedCookies = savedCookies.concat(window.StatisticalCookies);
                    } else {
                        aux1 = aux1 + "0";
                    }
                    if ($("#chkMarketing").prop("checked")) {
                        aux1 = aux1 + "1";
                        acceptedOptions.marketing = true;
                        savedCookies = savedCookies.concat(window.MarketingCookies);
                    } else {
                        aux1 = aux1 + "0";
                    }
                }

                // if unchecked checkbox removed cookie not in selected category
                if (aux1 !== "111") {
                    ResetCookies(savedCookies);
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
                    DrawCookiesResetButton(options);
                });
            }
            function ToggleChoices() {
                $('.cc-cookie-box form').slideToggle('slow');
            }
            function ExpandChoices() {
                $('.cc-cookie-box form').show();
            }
        };
    }
    return load();
})(jQuery);