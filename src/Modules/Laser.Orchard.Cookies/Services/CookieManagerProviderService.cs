using System;
using System.Collections.Generic;

namespace Laser.Orchard.Cookies.Services {
    public class CookieManagerProviderService : ICookieManagerProviderService {
        private readonly IEnumerable<ICookieManagerProvider> _providers;

        public CookieManagerProviderService(IEnumerable<ICookieManagerProvider> providers) {
            _providers = providers;
        }   

        public IList<CookieType> GetAcceptedCookieTypes() {
            foreach (var provider in _providers) {
                if (provider.IsValidProvider()) {
                    return provider.GetAcceptedCookieTypes();
                }
            }

            // If no valid provider is found, technical cookies only are accepted.
            var result = new List<CookieType>();
            // accepted by default
            result.Add(CookieType.Technical);
            return result;
        }
    }
}