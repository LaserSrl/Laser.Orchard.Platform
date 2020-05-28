using Laser.Orchard.NwazetIntegration.Models;
using Orchard.Caching;
using Orchard.ContentManagement;
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

        public CheckoutSettingsService(
            ISiteService siteService,
            ICacheManager cacheManager,
            ISignals signals) {

            _siteService = siteService;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        #region cache keys
        private const string _settingsCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.CheckoutSettingsService.Settings";
        #endregion

        public bool AuthenticationRequired {
            get { return Settings.CheckoutRequiresAuthentication; }
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