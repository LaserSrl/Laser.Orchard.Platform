using Laser.Orchard.Cookies.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.GoogleAnalytics.Services {
    public class AdditionalCookieProvider : ICookieProvider {
        public AdditionalCookieProvider() {
            DefaulStatisticalCookies = "_ga,_gid";
        }

        public string DefaulStatisticalCookies { get; set; }
        
        public IEnumerable<string> GetTechnicalCookies() {
            return new List<string>();
        }
        public IEnumerable<string> GetPreferencesCookies() {
            return new List<string>();
        }

        private IEnumerable<string> _statisticalCookies;

        public IEnumerable<string> GetStatisticalCookies() {
            if (_statisticalCookies == null) {
                List<string> defaulStatisticalCookies = new List<string>();
                if (!string.IsNullOrWhiteSpace(DefaulStatisticalCookies)) {
                    defaulStatisticalCookies = new List<string>(DefaulStatisticalCookies
                        .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }
                _statisticalCookies = defaulStatisticalCookies;
            }
            return _statisticalCookies;
        }

        public IEnumerable<string> GetMarketingCookies() {
            return new List<string>();
        }
    }
}