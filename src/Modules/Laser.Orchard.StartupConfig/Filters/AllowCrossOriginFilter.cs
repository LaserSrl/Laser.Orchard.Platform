using Laser.Orchard.StartupConfig.Models;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Mvc.Filters;
using Orchard.Settings;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Laser.Orchard.StartupConfig.Filters {
    public class AllowCrossOriginFilter : FilterProvider, IResultFilter {
        /*
         * Using System.Web.Mvc.HtmlHelper.AntiForgeryToken() in a view causes the
         * framework to add the 
         *   X-Frame-Options: SAMEORIGIN
         * header. This header prevents the page to work in iframes on most modern
         * browsers. Sometimes, this is not what we want. 
         * This filter allows suppressing that header.
         */
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;
        public AllowCrossOriginFilter(
            ICacheManager cacheManager,
            ISignals signals,
            ISiteService siteService) {

            _cacheManager = cacheManager;
            _signals = signals;
            _siteService = siteService;

            var settings = _cacheManager.Get(AllowCrossOriginSettingsPart.SettingsCacheKey, true, context => {
                context.Monitor(_signals.When(AllowCrossOriginSettingsPart.SettingsCacheKey));
                var part = _siteService.GetSiteSettings().As<AllowCrossOriginSettingsPart>();
                return part;
            });

            if (settings != null) {
                RemoveXFrameHeaderFrontEnd = settings.RemoveXFrameHeaderFrontEnd;
                RemoveXFrameHeaderBackEnd = settings.RemoveXFrameHeaderBackEnd;
                SetSameSiteNoneForAuthCookies = settings.SetSameSiteNoneForAuthCookies;
            }
        }

        private bool RemoveXFrameHeaderFrontEnd { get; set; }
        private bool RemoveXFrameHeaderBackEnd { get; set; }
        private bool SetSameSiteNoneForAuthCookies { get; set; }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
            if (AdminFilter.IsApplied(new RequestContext(filterContext.HttpContext, new RouteData()))) {
                if (RemoveXFrameHeaderBackEnd) {
                    HttpContext.Current.Response.Headers.Remove("X-Frame-Options");
                }
            } else {
                if (RemoveXFrameHeaderFrontEnd) {
                    HttpContext.Current.Response.Headers.Remove("X-Frame-Options");
                }
            }
            if (SetSameSiteNoneForAuthCookies) {
                if (HttpContext.Current.Response.Cookies.AllKeys.Contains(FormsAuthentication.FormsCookieName)) {
                    if (HttpContext.Current.Response.Cookies[FormsAuthentication.FormsCookieName].Secure) {
                        HttpContext.Current.Response.Cookies[FormsAuthentication.FormsCookieName].SameSite = SameSiteMode.None;
                    }
                }
            }
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) { }
    }
}