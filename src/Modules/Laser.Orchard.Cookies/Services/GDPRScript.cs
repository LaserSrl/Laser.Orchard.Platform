using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Web;

namespace Laser.Orchard.Cookies.Services {
    public class GDPRScript : IGDPRScript {
        private List<CookieType> orderedTypes = new List<CookieType>() { CookieType.Preference, CookieType.Statistical, CookieType.Marketing };
        private IEnumerable<ICookieGDPR> _cookies;
        public Localizer T { get; set; }

        public GDPRScript(IEnumerable<ICookieGDPR> cookies) {
            _cookies = cookies;
            T = NullLocalizer.Instance;
        }
        /// <summary>
        /// Get a script to create GDPR choice banner for the user.
        /// </summary>
        /// <returns></returns>
        public string GetBannerChoices() {
            string result = "";
            string checkedDisabled = "";
            // cicla sui tipi di cookie e crea il banner in modo da visualizzare solo le checkbox necessarie
            foreach (var cookieType in GetActiveCookieTypes()) {
                if(cookieType == CookieType.Technical) {
                    checkedDisabled = "checked disabled";
                } else {
                    checkedDisabled = "";
                }
                result += "<input type=\"checkbox\" id=\"chk" + cookieType.ToString() + "\" value=\"1\" " + checkedDisabled + "/><label for=\"chk" + cookieType.ToString() + "\">" + T(cookieType.ToString()) + "</label>";
            }
            return result;
        }
        /// <summary>
        /// Get choices of the user about cookies according to GDPR.
        /// </summary>
        /// <returns></returns>
        public IList<CookieType> GetCurrentGDPR() {
            List<CookieType> result = new List<CookieType>();
            var cookie = HttpContext.Current.Request.Cookies["cc_cookie_accept"];
            if (cookie != null) {
                foreach (string kv in cookie.Values) {
                    switch (kv) {
                        case "Preference":
                            result.Add(CookieType.Preference);
                            break;
                        case "Statistical":
                            result.Add(CookieType.Statistical);
                            break;
                        case "Marketing":
                            result.Add(CookieType.Marketing);
                            break;
                    }
                }
            }
            return result;
        }
        public string GetCurrentCookiePrefix() {
            string result = "cc_cookie_accept";
            var activeTypes = GetActiveCookieTypes();
            foreach (var cookieType in orderedTypes) {
                if (cookieType != CookieType.Technical && activeTypes.Contains(cookieType)) {
                    result += "_" + cookieType.ToString();
                }
            }
            result += ".";
            return result;
        }
        public int IsCookieAccepted() {
            int result = 0;
            var cookie = HttpContext.Current.Request.Cookies["cc_cookie_accept"];
            if(cookie != null) {
                if (cookie.Value.StartsWith(GetCurrentCookiePrefix())) {
                    result = 1;
                }
            }
            return result;
        }
        public IList<ICookieGDPR> GetAllowedCookies() {
            var result = new List<ICookieGDPR>();
            var types = GetAcceptedTypes();
            var accepted = false;
            foreach (var cookie in _cookies) {
                accepted = true;
                foreach (var cookieType in cookie.GetCookieTypes()) {
                    if (types.Contains(cookieType) == false && cookieType != CookieType.Technical) {
                        accepted = false;
                        break;
                    }
                }
                if (accepted) {
                    result.Add(cookie);
                }
            }
            return result;
        }
        ///// <summary>
        ///// Set choices of the user about cookies according to GDPR.
        ///// </summary>
        ///// <param name="cookieTypes"></param>
        //public void SetCurrentGDPR(IList<CookieType> cookieTypes) {
        //    throw new NotImplementedException();
        //}
        private IList<CookieType> GetActiveCookieTypes() {
            List<CookieType> result = new List<CookieType>();
            result.Add(CookieType.Technical);
            foreach (var cookie in _cookies) {
                foreach(var cookieType in cookie.GetCookieTypes()) {
                    if(result.Contains(cookieType) == false) {
                        result.Add(cookieType);
                    }
                }
            }
            return result;
        }
        private IList<CookieType> GetAcceptedTypes() {
            var result = new List<CookieType>();
            var cookie = HttpContext.Current.Request.Cookies["cc_cookie_accept"];
            if (cookie != null) {
                var arrVal = cookie.Value.Split('.');
                if (arrVal != null && arrVal.Length > 1) {
                    var arrCheck = arrVal[1].Split('_');
                    if (arrCheck.Length > 1 && arrCheck[1] == "1") {
                        result.Add(CookieType.Preference);
                    }
                    if (arrCheck.Length > 2 && arrCheck[2] == "1") {
                        result.Add(CookieType.Statistical);
                    }
                    if (arrCheck.Length > 3 && arrCheck[3] == "1") {
                        result.Add(CookieType.Marketing);
                    }
                }
            }
            return result;
        }
    }
}