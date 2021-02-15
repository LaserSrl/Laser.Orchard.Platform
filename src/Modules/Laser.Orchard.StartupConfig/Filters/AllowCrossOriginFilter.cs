using Laser.Orchard.StartupConfig.Models;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Mvc.Filters;
using Orchard.Security;
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
        private readonly ISslSettingsProvider _sslSettingsProvider;
        public AllowCrossOriginFilter(
            ICacheManager cacheManager,
            ISignals signals,
            ISiteService siteService,
            ISslSettingsProvider sslSettingsProvider) {

            _cacheManager = cacheManager;
            _signals = signals;
            _siteService = siteService;
            _sslSettingsProvider = sslSettingsProvider;

            var settings = _cacheManager.Get(AllowCrossOriginSettingsPart.SettingsCacheKey, true, context => {
                context.Monitor(_signals.When(AllowCrossOriginSettingsPart.SettingsCacheKey));
                var part = _siteService.GetSiteSettings().As<AllowCrossOriginSettingsPart>();
                return part;
            });

            if (settings != null) {
                RemoveXFrameHeaderFrontEnd = settings.RemoveXFrameHeaderFrontEnd;
                RemoveXFrameHeaderBackEnd = settings.RemoveXFrameHeaderBackEnd;
                SameSiteModeSetting = settings.CookieSameSiteMode;
            }

            RequireSSL = new Lazy<bool>(() => _sslSettingsProvider.GetRequiresSSL());
        }

        private bool RemoveXFrameHeaderFrontEnd { get; set; }
        private bool RemoveXFrameHeaderBackEnd { get; set; }
        private CookieSameSiteModeSetting SameSiteModeSetting { get; set; }
        private Lazy<bool> RequireSSL { get; set; }

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
            if (HttpContext.Current.Response.Cookies.AllKeys.Any()) {
                Action<HttpCookie> attributeOp;
                // Different SameSite attribute values have known issues with some browsers:
                // https://docs.microsoft.com/en-us/aspnet/samesite/system-web-samesite#known
                // https://devblogs.microsoft.com/aspnet/upcoming-samesite-cookie-changes-in-asp-net-and-asp-net-core/
                switch (SameSiteModeSetting) {
                    case CookieSameSiteModeSetting.Unspecified:
                        attributeOp = (c) => c.SameSite = (SameSiteMode)(-1);
                        break;
                    case CookieSameSiteModeSetting.None:
                        // set samesite = none only if the cookie is set as secure
                        attributeOp = (c) => {
                            if (!c.Secure) {
                                // see if we require SSL on all pages
                                c.Secure = RequireSSL.Value;
                            }
                            if (c.Secure) {
                                c.SameSite = SameSiteMode.None;
                            }
                        };
                        break;
                    case CookieSameSiteModeSetting.Lax:
                        attributeOp = (c) => c.SameSite = SameSiteMode.Lax;
                        break;
                    case CookieSameSiteModeSetting.Strict:
                        attributeOp = (c) => c.SameSite = SameSiteMode.Strict;
                        break;
                    case CookieSameSiteModeSetting.DontAlter:
                    default:
                        attributeOp = (c) => { };
                        break;
                }
                foreach (var key in HttpContext.Current.Response.Cookies.AllKeys) {
                    attributeOp(HttpContext.Current.Response.Cookies[key]);
                }
            }
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) { }
    }
}