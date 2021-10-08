using Laser.Orchard.Cookies.Providers;
using Laser.Orchard.GoogleAnalytics.Models;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.GoogleAnalytics.Services {
    public class AdditionalCookieProvider : ICookieProvider {
        private readonly IOrchardServices _orchardServices;

        public AdditionalCookieProvider(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;

            DefaulCookies = "_ga,_gid";

            googleAnalyticsSettings = _orchardServices.WorkContext.CurrentSite.As<GoogleAnalyticsSettingsPart>();
        }

        public GoogleAnalyticsSettingsPart googleAnalyticsSettings { get; set; }

        public string DefaulCookies { get; set; }

        private IEnumerable<string> _technicalCookies;

        public IEnumerable<string> GetTechnicalCookies() {
            if (googleAnalyticsSettings != null &&
                googleAnalyticsSettings.CookieLevel == CookieLevels.Technical &&
                _technicalCookies == null) {
                _technicalCookies = GetCookieDefault();
            } else {
                _technicalCookies = new List<string>();
            }
            return _technicalCookies;
        }

        private IEnumerable<string> _preferencesCookies;

        public IEnumerable<string> GetPreferencesCookies() {
            if (googleAnalyticsSettings != null &&
                 googleAnalyticsSettings.CookieLevel == CookieLevels.Preferences &&
                 _preferencesCookies == null) {
                _preferencesCookies = GetCookieDefault();
            } else {
                _preferencesCookies = new List<string>();
            }
            return _preferencesCookies;
        }

        private IEnumerable<string> _statisticalCookies;

        public IEnumerable<string> GetStatisticalCookies() {
            if (googleAnalyticsSettings != null && 
                googleAnalyticsSettings.CookieLevel == CookieLevels.Statistical && 
                _statisticalCookies == null) {
                 _statisticalCookies = GetCookieDefault();
            } else {
                _statisticalCookies = new List<string>();
            }
            return _statisticalCookies;
        }

        private IEnumerable<string> _marketingCookies;

        public IEnumerable<string> GetMarketingCookies() {
            if (googleAnalyticsSettings != null &&
                googleAnalyticsSettings.CookieLevel == CookieLevels.Marketing &&
                _marketingCookies == null) {
                _marketingCookies = GetCookieDefault();
            } else {
                _marketingCookies = new List<string>();
            }
            return _marketingCookies;
        }


        private IEnumerable<string> GetCookieDefault() {
            List<string> defaulCookies = new List<string>();
            if (!string.IsNullOrWhiteSpace(DefaulCookies)) {
                defaulCookies = new List<string>(DefaulCookies
                    .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }
            return defaulCookies;
        }
    }
}