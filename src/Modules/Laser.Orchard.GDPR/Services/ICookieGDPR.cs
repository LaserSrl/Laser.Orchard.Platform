using Laser.Orchard.GDPR.Extensions;
using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.GDPR.Services {
    public interface ICookieGDPR : IDependency {
        string GetCookieName();
        IList<CookieType> GetCookieTypes();
        //string GetScript(); // non serve: lo script viene gestito dal modulo che genera il cookie interrogando IGDPRScript; forse si può usare per standardizzare
    }
}