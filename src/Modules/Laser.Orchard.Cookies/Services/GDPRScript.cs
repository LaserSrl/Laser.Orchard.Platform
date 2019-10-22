using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Linq;
using Orchard;
using Laser.Orchard.Cookies.Models;
using Orchard.ContentManagement;

namespace Laser.Orchard.Cookies.Services {
    public class GDPRScript : IGDPRScript {
        private readonly IOrchardServices _orchardServices;
        private List<CookieType> orderedTypes = new List<CookieType>() { CookieType.Technical, CookieType.Preferences, CookieType.Statistical, CookieType.Marketing };
        private IEnumerable<ICookieGDPR> _cookies;
        public Localizer T { get; set; }

        public GDPRScript(IEnumerable<ICookieGDPR> cookies, IOrchardServices orchardServices) {
            _cookies = cookies;
            _orchardServices = orchardServices;
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
            var settings = _orchardServices.WorkContext.CurrentSite.As<CookieSettingsPart>();
            // esplicita le traduzioni delle tipologie di cookie a beneficio del tool di traduzione
            var Technical = T("Technical");
            var Preferences = T("Preferences");
            var Statistical = T("Statistical");
            var Marketing = T("Marketing");
            // cicla sui tipi di cookie e crea il banner in modo da visualizzare solo le checkbox necessarie
            foreach (var cType in orderedTypes) {
                if (activeCookieTypes.Contains(cType)) {
                    switch (cType) {
                        case CookieType.Technical:
                            checkedDisabled = "checked disabled";
                            break;
                        case CookieType.Preferences:
                            checkedDisabled = (settings.defaultValuePreferences)? "checked" : "";
                            break;
                        case CookieType.Statistical:
                            checkedDisabled = (settings.defaultValueStatistical) ? "checked" : "";
                            break;
                        case CookieType.Marketing:
                            checkedDisabled = (settings.defaultValueMarketing) ? "checked" : "";
                            break;
                    }
                    result += "<label for=\"chk" + cType.ToString() + "\" style=\"margin-right:0.5em;\"><input type=\"checkbox\" id=\"chk" + cType.ToString() + "\" value=\"1\" " + checkedDisabled + " style=\"margin-right:0.5em;\"/>" + T(cType.ToString()) + "</label>";
                }
            }
            return result;
        }
        public string GetCurrentCookiePrefix() {
            string result = "";
            var textToHash = new StringBuilder("cc_cookie_accept");
            var orderedCookies = from cc in _cookies orderby cc.GetCookieName() select cc;
            foreach (var cookie in orderedCookies) {
                textToHash.Append(cookie.GetCookieName());
                foreach(var ct in orderedTypes) {
                    if (cookie.GetCookieTypes().Contains(ct)) {
                        textToHash.Append(ct.ToString());
                    }
                    textToHash.AppendLine();
                }
            }
            var md5 = MD5.Create();
            var arr = md5.ComputeHash(Encoding.UTF8.GetBytes(textToHash.ToString()));
            result =  BitConverter.ToString(arr).Replace("-", "") + ".";
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
            IList<CookieType> types = new List<CookieType>();
            var settings = _orchardServices.WorkContext.CurrentSite.As<CookieSettingsPart>();
            if (settings.DisableCookieGDPRManagement) {
                types.Add(CookieType.Technical);
                types.Add(CookieType.Preferences);
                types.Add(CookieType.Statistical);
                types.Add(CookieType.Marketing);
            } else {
                types = GetAcceptedCookieTypes();
            }
            foreach (var cookie in _cookies) {
                foreach (var cookieType in cookie.GetCookieTypes()) {
                    if (types.Contains(cookieType)) {
                        result.Add(cookie);
                        break;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Get if cookie module is acceptable according to current user's choices. 
        /// </summary>
        /// <param name="cookieModule"></param>
        /// <returns></returns>
        public bool IsAcceptableForUser(ICookieGDPR cookieModule) {
            var result = false;
            var okCookies = GetAcceptedCookieTypes();
            foreach(var ct in cookieModule.GetCookieTypes()) {
                if(okCookies.Contains(ct)) {
                    result = true;
                    break;
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
        public IList<CookieType> GetActiveCookieTypes() {
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
        public IList<CookieType> GetAcceptedCookieTypes() {
            var result = new List<CookieType>();
            // accepted by default
            result.Add(CookieType.Technical);

            var cookie = HttpContext.Current.Request.Cookies["cc_cookie_accept"];
            if (cookie != null) {
                var arrVal = cookie.Value.Split('.');
                if (arrVal != null && arrVal.Length > 1) {
                    var arrCheck = arrVal[1];
                    if (arrCheck.Length > 0 && arrCheck[0] == '1') {
                        result.Add(CookieType.Preferences);
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