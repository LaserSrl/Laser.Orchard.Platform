using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.Cookies.Services {
    public interface IGDPRScript : IDependency {
        string GetBannerChoices();
        string GetCurrentCookiePrefix();
        int IsCookieAccepted();
        IList<ICookieGDPR> GetAllowedCookies();
        bool IsAcceptableForUser(ICookieGDPR cookieModule);
        IList<CookieType> GetActiveCookieTypes();
        IList<CookieType> GetAcceptedCookieTypes();
    }
}