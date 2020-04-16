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
using System.Web;
using Orchard.DisplayManagement;
using System.Collections.Generic;

namespace Laser.Orchard.GoogleAnalytics.Filters {
    [OrchardFeature("Laser.Orchard.GoogleAnalytics")]
    public class GoogleAnalyticsFilter : FilterProvider, IResultFilter {
        private readonly IResourceManager _resourceManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IGoogleAnalyticsCookie _googleAnalyticsCookie;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly dynamic _shapeFactory;

        public GoogleAnalyticsFilter(
            IResourceManager resourceManager, 
            IOrchardServices orchardServices, 
            IGoogleAnalyticsCookie googleAnalyticsCookie,
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals,
            IShapeFactory shapeFactory) {

            _resourceManager = resourceManager;
            _orchardServices = orchardServices;
            _googleAnalyticsCookie = googleAnalyticsCookie;
            _workContextAccessor = workContextAccessor;
            _cacheManager = cacheManager;
            _signals = signals;
            _shapeFactory = shapeFactory;
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

            // add the <noscript> element for tagmanager
            if (SettingsPart != null
                && !string.IsNullOrWhiteSpace(SettingsPart.GoogleAnalyticsKey)
                && ((isAdmin && SettingsPart.TrackOnAdmin) || (!isAdmin && SettingsPart.TrackOnFrontEnd))) {

                var snippet = _googleAnalyticsCookie.GetNoScript();
                if (!string.IsNullOrWhiteSpace(snippet)) {
                    var noscript = new HtmlString(snippet);
                    // write that to the top of the page, immediately after the opening <body> tag
                    var body = _workContextAccessor.GetContext()
                        .Layout.Body;
                    if (body.Items is IList<object>) {
                        body.Items.Insert(0, noscript);
                    } else {
                        // this probably won't let us place the snippet in the correct
                        // position. On the other hand, with the existing implementations body.Items 
                        // is always a List. This branch is here for safety, so that if ever we have
                        // a different implementation of body.Items this will not crash.
                        body.Add(noscript, "1");
                    }
                }
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