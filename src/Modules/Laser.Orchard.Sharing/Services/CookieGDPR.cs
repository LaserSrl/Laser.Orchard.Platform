using Laser.Orchard.Cookies.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Cookies;

namespace Laser.Orchard.Sharing.Services {
    public class CookieGDPR : ICookieGDPR {
        public string GetCookieName() {
            return "AddThis";
        }

        public IList<CookieType> GetCookieTypes() {
            return new List<CookieType>() { CookieType.Marketing };
        }

        public string GetFootScript(IList<CookieType> allowedTypes) {
            // html, js e css sono nelle partial view, il check per capire se aggiungerle o no è nel driver
            return "";
        }

        public string GetHeadScript(IList<CookieType> allowedTypes) {
            return string.Empty;
        }
    }
}