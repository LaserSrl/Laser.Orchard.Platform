using System.Collections.Generic;
using System.Web;

namespace Laser.Orchard.Cookies.Services {
    public class CookieCutterProvider : ICookieManagerProvider {
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

        public bool IsValidProvider() {
            var cookie = HttpContext.Current.Request.Cookies["cc_cookie_accept"];

            return (cookie != null);
        }
    }
}