using Laser.Orchard.Cookies.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Cookies.Services {
    public class CookieServices : ICookieService {
        private readonly IEnumerable<ICookieProvider> _cookieProvider;
        public CookieServices(
            IEnumerable<ICookieProvider> cookieProvider) {
            _cookieProvider = cookieProvider;
        }
        public IEnumerable<string> GetMarketingCookies() {
            return _cookieProvider
                .SelectMany(c => c.GetMarketingCookies())
                .Distinct().ToList();
        }

        public IEnumerable<string> GetPreferencesCookies() {
            return _cookieProvider
                .SelectMany(c => c.GetPreferencesCookies())
                .Distinct().ToList();
        }

        public IEnumerable<string> GetStatisticalCookies() {
            return _cookieProvider
                .SelectMany(c => c.GetStatisticalCookies())
                .Distinct().ToList();
        }

        public IEnumerable<string> GetTechnicalCookies() {
            return _cookieProvider
                .SelectMany(c => c.GetTechnicalCookies())
                .Distinct().ToList();
        }
    }
}