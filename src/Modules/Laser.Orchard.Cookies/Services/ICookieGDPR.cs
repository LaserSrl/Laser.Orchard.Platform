using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.Cookies.Services {
    public interface ICookieGDPR : IDependency {
        string GetCookieName();
        IList<CookieType> GetCookieTypes();
        string GetScript();
    }
}