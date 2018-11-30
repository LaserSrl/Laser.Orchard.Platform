using Laser.Orchard.GDPR.Extensions;
using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.GDPR.Services {
    public interface IGDPRScript : IDependency {
        List<CookieType> GetCurrentGDPR();
        void SetCurrentGDPR(List<CookieType> cookieTypes);
        string GetBannerChoice();
    }
}