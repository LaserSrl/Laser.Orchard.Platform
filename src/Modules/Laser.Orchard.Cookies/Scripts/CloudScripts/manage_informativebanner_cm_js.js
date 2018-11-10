setTimeout(function () {
	jQuery(document).ready(function () {
		addInformativeCookieBanner();
		checkConsentCookieAndShowFascia();

		jQuery(document).click(function (e) {
			if (isInsideTheBanner(e)) {
				checkCookieAndCloseFascia();
			}
		});

		jQuery("a#info").click(function () {
			consentCookie.setConsentCookieIfNotExist("YES");
		});
	});
}, 600);

function checkConsentCookieAndShowFascia() {
	if (consentCookie.getCookie("consentCookie") == "") {
		showfascia();
	}
}

function showfascia() {
	jQuery(".cookiefascia").animate({
		height : "toggle"
	});
}

function checkCookieAndCloseFascia() {
	consentCookie.setConsentCookieIfNotExist("YES");
	jQuery("#cookiefascia").animate({
		height : "0"
	}, 400);
}

function closefascia(e) {
	consentCookie.setCookie("consentCookie", "YES", 365);
	jQuery("#cookiefascia").animate({
		height : "0"
	}, 400);
}

function isInsideTheBanner(e) {
	var classn = e.target.className;
	if (classn != "cookiefascia" && classn != "cfcontent" && classn != "cleft" && classn != "ctoptext" && classn != "cbottomtext" && classn != "cimage") {
		return true;
	}
	return false;
}