using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Cookies.Providers {
    public interface ICookieProvider : IDependency {
        IEnumerable<string> GetTechnicalCookies();
        IEnumerable<string> GetPreferencesCookies();
        IEnumerable<string> GetStatisticalCookies();
        IEnumerable<string> GetMarketingCookies();
    }
}