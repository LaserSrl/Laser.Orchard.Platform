using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.Cookies.Services {
    public interface IGDPRScript : IDependency {
        IList<CookieType> GetCurrentGDPR();
        //void SetCurrentGDPR(IList<CookieType> cookieTypes);
        string GetBannerChoices();
        string GetCurrentCookiePrefix();
        int IsCookieAccepted();
        IList<ICookieGDPR> GetAllowedCookies();
    }
}