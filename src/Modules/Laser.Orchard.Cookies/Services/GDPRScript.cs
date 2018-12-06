using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Web;

namespace Laser.Orchard.Cookies.Services {
    public class GDPRScript : IGDPRScript {
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
            // cicla sui tipi di cookie e crea il banner in modo da visualizzare solo le checkbox necessarie
            foreach (var cookieType in GetActiveCookieTypes()) {
                if(cookieType != CookieType.Technical) {
                    result += "<input type=\"checkbox\" id=\"chk" + cookieType.ToString() + "\" value=\"1\"/><label for=\"chk" + cookieType.ToString() + "\">" + T(cookieType.ToString()) + "</label>";
                }
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
    }
}