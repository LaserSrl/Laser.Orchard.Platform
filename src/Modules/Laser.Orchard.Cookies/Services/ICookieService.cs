using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Cookies.Services {
    public interface ICookieService : IDependency {
        IEnumerable<string> GetTechnicalCookies();
        IEnumerable<string> GetPreferencesCookies();
        IEnumerable<string> GetStatisticalCookies();
        IEnumerable<string> GetMarketingCookies();
    }
}