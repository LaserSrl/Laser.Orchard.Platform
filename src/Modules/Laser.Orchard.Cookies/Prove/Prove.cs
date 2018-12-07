using Laser.Orchard.Cookies.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Cookies.Prove {
    public class Prova1 : ICookieGDPR {
        public string GetCookieName() {
            return "Prova1";
        }

        public IList<CookieType> GetCookieTypes() {
            var result = new List<CookieType>();
            result.Add(CookieType.Statistical);
            result.Add(CookieType.Preference);
            result.Add(CookieType.Technical);
            result.Add(CookieType.Marketing);
            return result;
        }

        public string GetScript() {
            return "<script type=\"text/javascript\" src=\"//prova.it/js/prova_widget.js#key=55\"></script>";
        }
    }
}