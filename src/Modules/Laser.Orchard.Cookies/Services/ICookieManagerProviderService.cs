using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.Cookies.Services {
    public interface ICookieManagerProviderService : IDependency {
        IList<CookieType> GetAcceptedCookieTypes();
    }
}
