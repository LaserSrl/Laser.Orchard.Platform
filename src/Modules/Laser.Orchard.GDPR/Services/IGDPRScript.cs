using Laser.Orchard.GDPR.Extensions;
using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.GDPR.Services {
    public interface IGDPRScript : IDependency {
        IList<CookieType> GetCurrentGDPR();
        void SetCurrentGDPR(IList<CookieType> cookieTypes);
        string GetBannerChoice();
    }
}