using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Web;

namespace Laser.Orchard.Cookies.Services {
    public class GDPRScript : IGDPRScript {
        private List<CookieType> orderedTypes = new List<CookieType>() { CookieType.Technical, CookieType.Preference, CookieType.Statistical, CookieType.Marketing };
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
            var activeCookieTypes = GetActiveCookieTypes();
            // cicla sui tipi di cookie e crea il banner in modo da visualizzare solo le checkbox necessarie
            foreach (var cookieType in orderedTypes) {
                if (activeCookieTypes.Contains(cookieType)) {
                    if (cookieType == CookieType.Technical) {
                        checkedDisabled = "checked disabled";
                    } else {
                        checkedDisabled = "";
                    }
                    result += "<input type=\"checkbox\" id=\"chk" + cookieType.ToString() + "\" value=\"1\" " + checkedDisabled + "/><label for=\"chk" + cookieType.ToString() + "\">" + T(cookieType.ToString()) + "</label>";
                }
            }
            return result;
        }
        public string GetCurrentCookiePrefix() {
            string result = "cc_cookie_accept";
            var activeCookieTypes = GetActiveCookieTypes();
            foreach (var cookieType in orderedTypes) {
                if (cookieType != CookieType.Technical && activeCookieTypes.Contains(cookieType)) {
                    result += "_" + cookieType.ToString();
                }
            }
            result += ".";
            return result;
        }
        /// <summary>
        /// Check whether current user made his choice about cookies according to GDPR.
        /// If new cookie types were introduced after user's choice, then user has to be notified as if he never made his choice before.
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Get all allowed cookies according to current user's choice.
        /// </summary>
        /// <returns></returns>
        public IList<ICookieGDPR> GetAllowedCookies() {
            var result = new List<ICookieGDPR>();
            var types = GetAcceptedCookieTypes();
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
        /// <summary>
        /// Get cookie types accepted by the current user.
        /// </summary>
        /// <returns></returns>
        private IList<CookieType> GetAcceptedCookieTypes() {
            var result = new List<CookieType>();
            var cookie = HttpContext.Current.Request.Cookies["cc_cookie_accept"];
            if (cookie != null) {
                var arrVal = cookie.Value.Split('.');
                if (arrVal != null && arrVal.Length > 1) {
                    var arrCheck = arrVal[1];
                    if (arrCheck.Length > 0 && arrCheck[0] == '1') {
                        result.Add(CookieType.Preference);
                    }
                    if (arrCheck.Length > 1 && arrCheck[1] == '1') {
                        result.Add(CookieType.Statistical);
                    }
                    if (arrCheck.Length > 2 && arrCheck[2] == '1') {
                        result.Add(CookieType.Marketing);
                    }
                }
            }
            return result;
        }
    }
}