using Laser.Orchard.Cookies.Services;
using System.Collections.Generic;
using Laser.Orchard.Cookies;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.HubSpot.Services {
    [OrchardFeature("Laser.Orchard.HubSpot")]
    public class HubSpotCookieGDPR : ICookieGDPR {
        public string GetCookieName() {
            return "HubSpot";
        }

        public IList<CookieType> GetCookieTypes() {
            return new List<CookieType>() { CookieType.Marketing };
        }

        public string GetScript() {
            string result = "<!-- Start of HubSpot Embed Code -->\r\n<script type=\"text/javascript\" id=\"hs - script - loader\" async defer src=\"//js.hs-scripts.com/2170079.js\"></script>\r\n<!-- End of HubSpot Embed Code -->";
            return result;
        }
    }
}