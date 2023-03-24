using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.Cookies.Services {
    public interface ICookieManagerProvider : IDependency {
        bool IsValidProvider();
        IList<CookieType> GetAcceptedCookieTypes();
    }
}
