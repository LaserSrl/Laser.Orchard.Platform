using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.Cookies.Services {
    public interface ICookieGDPR : IDependency {
        string GetCookieName();
        IList<CookieType> GetCookieTypes();
        /// <summary>
        /// Get the script to use according to cookieTypes allowed (Technical + accepted by the user).
        /// </summary>
        /// <param name="allowedTypes"></param>
        /// <returns></returns>
        string GetFootScript(IList<CookieType> allowedTypes);
        /// <summary>
        /// Get the script to use according to cookieTypes allowed (Technical + accepted by the user).
        /// </summary>
        /// <param name="allowedTypes"></param>
        /// <returns></returns>
        string GetHeadScript(IList<CookieType> allowedTypes);
    }
}