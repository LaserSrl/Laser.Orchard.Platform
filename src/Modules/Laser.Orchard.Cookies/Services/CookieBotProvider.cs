using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web;

namespace Laser.Orchard.Cookies.Services {
    public class CookieBotProvider : ICookieManagerProvider {
        public IList<CookieType> GetAcceptedCookieTypes() {
            var result = new List<CookieType>();
            // accepted by default
            result.Add(CookieType.Technical);

            var cookie = HttpContext.Current.Request.Cookies["CookieConsent"];
            if (cookie != null) {
                // Cookie value is something like
                // {stamp:%27A2srdjbyYZV/cpAvl2+pNv+vHoIdvQa+YI/PaYCuFIayzseFQuwoPA==%27%2Cnecessary:true%2Cpreferences:false%2Cstatistics:false%2Cmarketing:false%2Cmethod:%27explicit%27%2Cver:2%2Cutc:1677250338212%2Cregion:%27it%27}
                // It's a url encoded Json, so it needs to be decoded first.
                var decodedValue = HttpUtility.UrlDecode(cookie.Value);
                dynamic deserializedValue = JsonConvert.DeserializeObject(decodedValue);
                // Techinal cookies are usually accepted by default, but as long as they're inside this cookie, they're read from it.
                if (deserializedValue.necessary != null && deserializedValue.necessary.Value) {
                    result.Add(CookieType.Technical);
                }
                if (deserializedValue.preferences != null && deserializedValue.preferences.Value) {
                    result.Add(CookieType.Preferences);
                }
                if (deserializedValue.statistics != null && deserializedValue.statistics.Value) {
                    result.Add(CookieType.Statistical);
                }
                if (deserializedValue.marketing != null && deserializedValue.marketing.Value) {
                    result.Add(CookieType.Marketing);
                }
            }

            return result;
        }

        public bool IsValidProvider() {
            var cookie = HttpContext.Current.Request.Cookies["CookieConsent"];

            return (cookie != null);
        }
    }
}