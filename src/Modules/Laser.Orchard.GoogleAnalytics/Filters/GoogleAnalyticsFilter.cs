using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Filters;
using Orchard.UI.Resources;
using Laser.Orchard.GoogleAnalytics.Models;
using OUI = Orchard.UI;
using Laser.Orchard.GoogleAnalytics.Services;
using Orchard.Caching;

namespace Laser.Orchard.GoogleAnalytics.Filters {
    [OrchardFeature("Laser.Orchard.GoogleAnalytics")]
    public class GoogleAnalyticsFilter : FilterProvider, IResultFilter {
        private readonly IResourceManager _resourceManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IGoogleAnalyticsCookie _googleAnalyticsCookie;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        public GoogleAnalyticsFilter(
            IResourceManager resourceManager, 
            IOrchardServices orchardServices, 
            IGoogleAnalyticsCookie googleAnalyticsCookie,
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals) {

            _resourceManager = resourceManager;
            _orchardServices = orchardServices;
            _googleAnalyticsCookie = googleAnalyticsCookie;
            _workContextAccessor = workContextAccessor;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        #region IResultFilter Members

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult == null)
                return;

            //Determine if we're on an admin page
            bool isAdmin = OUI.Admin.AdminFilter.IsApplied(filterContext.RequestContext);
            // This is designed to only run in the admin, because frontend scripts are
            // handled by the module for GDPR cookies
            if (SettingsPart != null 
                && SettingsPart.TrackOnAdmin 
                && isAdmin 
                && !string.IsNullOrWhiteSpace(SettingsPart.GoogleAnalyticsKey)) {
                // Register Google's new, recommended asynchronous universal analytics script to the header
                // (or the tag manager script if we have that configuration)
                _resourceManager.RegisterHeadScript(
                    _googleAnalyticsCookie.GetHeadScript(_googleAnalyticsCookie.GetCookieTypes()));
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }

        #endregion

        private GoogleAnalyticsSettingsPart SettingsPart {
            get {
                return _cacheManager.Get(Constants.SiteSettingsCacheKey, true, ctx => {
                    // check whether we should invalidate the cache
                    ctx.Monitor(_signals.When(Constants.SiteSettingsEvictSignal));
                    return _workContextAccessor
                        .GetContext()
                        .CurrentSite
                        .As<GoogleAnalyticsSettingsPart>();
                });
            }
        }
    }
}