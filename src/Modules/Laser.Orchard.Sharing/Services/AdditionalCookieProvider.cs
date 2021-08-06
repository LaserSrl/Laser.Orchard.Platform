using Laser.Orchard.Cookies.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Sharing.Services {
    public class AdditionalCookieProvider : ICookieProvider {
        public AdditionalCookieProvider() {
            DefaultMarketingCookies = "__atuvc,__atuvs,__atssc,AddThis";
        }

        public string DefaultMarketingCookies { get; set; }
        
        public IEnumerable<string> GetTechnicalCookies() {
            return new List<string>();
        }
        public IEnumerable<string> GetPreferencesCookies() {
            return new List<string>();
        }

        public IEnumerable<string> GetStatisticalCookies() {
            return new List<string>();
        }

        private IEnumerable<string> _marketingCookies;

        public IEnumerable<string> GetMarketingCookies() {
            if (_marketingCookies == null) {
                List<string> defaulMarketingCookies = new List<string>();
                if (!string.IsNullOrWhiteSpace(DefaultMarketingCookies)) {
                    defaulMarketingCookies = new List<string>(DefaultMarketingCookies
                        .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }
                _marketingCookies = defaulMarketingCookies;
            }
            return _marketingCookies;
        }
    }
}