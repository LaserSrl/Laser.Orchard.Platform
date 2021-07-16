using Laser.Orchard.Cookies.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Accessibility.Services {
    public class AdditionalCookieProvider : ICookieProvider {
        public AdditionalCookieProvider() {
            DefaultTechnicalCookies = "Accessibility,AccessibilitySize";
        }

        public string DefaultTechnicalCookies { get; set; }

        private IEnumerable<string> _technicalCookies;

        public IEnumerable<string> GetTechnicalCookies() {
            if (_technicalCookies == null) {
                List<string> defaulTechnicalCookies = new List<string>();
                if (!string.IsNullOrWhiteSpace(DefaultTechnicalCookies)) {
                    defaulTechnicalCookies = new List<string>(DefaultTechnicalCookies
                        .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }
                _technicalCookies = defaulTechnicalCookies;
            }
            return _technicalCookies;
        }
        public IEnumerable<string> GetPreferencesCookies() {
            return new List<string>();
        }

        public IEnumerable<string> GetStatisticalCookies() {
            return new List<string>();
        }
        
        public IEnumerable<string> GetMarketingCookies() {
            return new List<string>();
        }
    }
}