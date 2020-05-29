using Laser.Orchard.NwazetIntegration.Models;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class CheckoutSettingsService : ICheckoutSettingsService {

        private readonly ISiteService _siteService;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IWorkContextAccessor _workContextAccessor;

        public CheckoutSettingsService(
            ISiteService siteService,
            ICacheManager cacheManager,
            ISignals signals,
            IWorkContextAccessor workContextAccessor) {

            _siteService = siteService;
            _cacheManager = cacheManager;
            _signals = signals;
            _workContextAccessor = workContextAccessor;
        }

        #region cache keys
        private const string _settingsCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.CheckoutSettingsService.Settings";
        #endregion

        public bool AuthenticationRequired {
            get { return Settings.CheckoutRequiresAuthentication; }
        }

        public bool UserMayCheckout(IUser user = null) {
            // centralize verification for the "permission" to checkout. This way
            // we can safely expand this condition later, possibly by building here 
            // an extension point.
            return !((user ?? _workContextAccessor.GetContext().CurrentUser) == null
                && AuthenticationRequired);
        }

        private CheckoutSettingsPart Settings {
            get {
                return GetFromCache(_settingsCacheKey, () => {
                    return _siteService.GetSiteSettings()
                        .As<CheckoutSettingsPart>();
                });
            }
        }
        private T GetFromCache<T>(string cacheKey, Func<T> method) {
            return _cacheManager.Get(cacheKey, true, ctx => {
                // invalidation signal 
                ctx.Monitor(_signals.When(Constants.CheckoutSettingsCacheEvictSignal));
                // cache
                return method();
            });
        }
    }
}