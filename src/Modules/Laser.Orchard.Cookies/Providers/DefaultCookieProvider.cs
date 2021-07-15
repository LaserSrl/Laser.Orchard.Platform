using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Cookies.Providers {
    public class DefaultCookieProvider : ICookieProvider {
        public DefaultCookieProvider() {

            // We may use AutoFac to override the default cookies:
            /*
             <component instance-scope="per-lifetime-scope"
                       type="Laser.Orchard.Cookies.Providers.DefaultCookieProvider, Laser.Orchard.Cookies"
                       service="Laser.Orchard.Cookies.Providers.ICookieProvider, Laser.Orchard.Cookies">
                <properties>
                    <property name="DefaultTechnicalCookies" value="DefaultTechnicalCookies_1, DefaultTechnicalCookies_2" />
                </properties>
            </component>
             */

            DefaultTechnicalCookies = "PoliciesAnswers,cc_cookie_accept,cultureData,.ASPXAUTH,ASP.NET_SessionId";
        }

        public string DefaultTechnicalCookies { get; set; }
        public string AdditionalTechnicalCookies { get; set; }
        public string DefaultPreferencesCookies { get; set; }
        public string AdditionalPreferencesCookies { get; set; }
        public string DefaulStatisticalCookies { get; set; }
        public string AdditionalStatisticalCookies { get; set; }
        public string DefaultMarketingCookies { get; set; }
        public string AdditionalMarketingCookies { get; set; }

        private IEnumerable<string> _technicalCookies;

        public IEnumerable<string> GetTechnicalCookies() {
            if (_technicalCookies == null) {
                List<string> defaulTechnicalCookies = new List<string>();
                if (!string.IsNullOrWhiteSpace(DefaultTechnicalCookies)) {
                    defaulTechnicalCookies = new List<string>(DefaultTechnicalCookies
                        .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }

                List<string> additionalTechnicalCookies = new List<string>();
                if (!string.IsNullOrWhiteSpace(AdditionalTechnicalCookies)) {
                    additionalTechnicalCookies = new List<string>(AdditionalTechnicalCookies
                        .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }
                _technicalCookies = defaulTechnicalCookies.Concat(additionalTechnicalCookies);
            }
            return _technicalCookies;
        }

        private IEnumerable<string> _preferencesCookies;

        public IEnumerable<string> GetPreferencesCookies() {
            if (_preferencesCookies == null) {
                List<string> defaultPreferencesCookies = new List<string>();
                if (!string.IsNullOrWhiteSpace(DefaultPreferencesCookies)) {
                    defaultPreferencesCookies = new List<string>(DefaultPreferencesCookies
                        .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }

                List<string> additionaltPreferencesCookies = new List<string>();
                if (!string.IsNullOrWhiteSpace(AdditionalPreferencesCookies)) {
                    additionaltPreferencesCookies = new List<string>(AdditionalPreferencesCookies
                        .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }
                _preferencesCookies = defaultPreferencesCookies.Concat(additionaltPreferencesCookies);
            }
            return _preferencesCookies;
        }

        private IEnumerable<string> _statisticalCookies;

        public IEnumerable<string> GetStatisticalCookies() {
            if (_statisticalCookies == null) {
                List<string> defaulStatisticalCookies = new List<string>();
                if (!string.IsNullOrWhiteSpace(DefaulStatisticalCookies)) {
                    defaulStatisticalCookies = new List<string>(DefaulStatisticalCookies
                        .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }

                List<string> additionalStatisticalCookies = new List<string>();
                if (!string.IsNullOrWhiteSpace(AdditionalStatisticalCookies)) {
                    additionalStatisticalCookies = new List<string>(AdditionalStatisticalCookies
                        .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }
                _statisticalCookies = defaulStatisticalCookies.Concat(additionalStatisticalCookies);
            }
            return _statisticalCookies;
        }

        private IEnumerable<string> _marketingCookies;

        public IEnumerable<string> GetMarketingCookies() {
            if (_marketingCookies == null) {
                List<string> defaulMarketingCookies = new List<string>();
                if (!string.IsNullOrWhiteSpace(DefaultMarketingCookies)) {
                    defaulMarketingCookies = new List<string>(DefaultMarketingCookies
                        .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }

                List<string> additionalMarketingCookies = new List<string>();
                if (!string.IsNullOrWhiteSpace(AdditionalMarketingCookies)) {
                    additionalMarketingCookies = new List<string>(AdditionalMarketingCookies
                        .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }
                _marketingCookies = defaulMarketingCookies.Concat(additionalMarketingCookies);
            }
            return _marketingCookies;
        }
    }
}