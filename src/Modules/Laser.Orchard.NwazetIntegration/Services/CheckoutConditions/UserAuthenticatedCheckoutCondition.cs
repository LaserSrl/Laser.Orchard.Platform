using Laser.Orchard.NwazetIntegration.Models;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.NwazetIntegration.Services.CheckoutConditions {
    public class UserAuthenticatedCheckoutCondition
        : ICheckoutCondition {

        private readonly ISiteService _siteService;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly INotifier _notifier;

        public UserAuthenticatedCheckoutCondition(
            ISiteService siteService,
            ICacheManager cacheManager,
            ISignals signals,
            IWorkContextAccessor workContextAccessor,
            INotifier notifier) {

            _siteService = siteService;
            _cacheManager = cacheManager;
            _signals = signals;
            _workContextAccessor = workContextAccessor;
            _notifier = notifier;

            T = NullLocalizer.Instance;

            Url = new UrlHelper(_workContextAccessor.GetContext().HttpContext.Request.RequestContext);
        }
        public Localizer T { get; set; }
        public int Priority => 10;

        public bool UserMayCheckout(IUser user, out ActionResult redirect) {
            // centralize verification for the "permission" to checkout. This way
            // we can safely expand this condition later, possibly by building here 
            // an extension point.
            redirect = new RedirectResult(Url
                .Action("AccessDenied", "Account", new { area = "Orchard.Users" }));
            var mayCheckout = UserMayCheckout(user);

            return mayCheckout;
        }

        public bool UserMayCheckout(IUser user) {
            var mayCheckout = !((user ?? _workContextAccessor.GetContext().CurrentUser) == null
                && AuthenticationRequired);
            if (!mayCheckout) {
                _notifier.Warning(T("<a href=\"{0}\">Please log on to complete your order.</a>",
                    Url.Action("LogOn", "Account", new { area = "Orchard.Users" })));
            }
            return mayCheckout;
        }

        private UrlHelper Url { get; set; }

        #region cache keys
        private const string _settingsCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.CheckoutSettingsService.Settings";
        #endregion
        
        private CheckoutSettingsPart Settings {
            get {
                return GetFromCache(_settingsCacheKey, () => {
                    return _siteService.GetSiteSettings()
                        .As<CheckoutSettingsPart>();
                });
            }
        }

        private bool AuthenticationRequired {
            get { return Settings.CheckoutRequiresAuthentication; }
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