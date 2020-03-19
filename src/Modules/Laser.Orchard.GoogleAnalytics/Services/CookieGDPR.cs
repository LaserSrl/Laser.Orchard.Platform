using Laser.Orchard.Cookies.Services;
using System.Collections.Generic;
using System.Web;
using Laser.Orchard.Cookies;
using System.Text;
using Orchard;
using Orchard.ContentManagement;
using OUI = Orchard.UI;
using Laser.Orchard.GoogleAnalytics.Models;
using Orchard.Caching;

namespace Laser.Orchard.GoogleAnalytics.Services {
    public interface IGoogleAnalyticsCookie : ICookieGDPR {
        string GetNoScript();
    }
    public class CookieGDPR : IGoogleAnalyticsCookie {
        private readonly IOrchardServices _orchardServices;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        public CookieGDPR(
            IOrchardServices orchardServices,
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals) {

            _orchardServices = orchardServices;
            _workContextAccessor = workContextAccessor;
            _cacheManager = cacheManager;
            _signals = signals;
        }
        public string GetCookieName() {
            return "Google Analytics";
        }

        public IList<CookieType> GetCookieTypes() {
            return new List<CookieType>() { CookieType.Technical, CookieType.Statistical };
        }

        public string GetHeadScript(IList<CookieType> allowedTypes) {
            //Determine if we're on an admin page
            bool isAdmin = OUI.Admin.AdminFilter.IsApplied(HttpContext.Current.Request.RequestContext);

            //Get our part data/record if available for rendering scripts
            if (SettingsPart == null
                || string.IsNullOrWhiteSpace(SettingsPart.GoogleAnalyticsKey)
                || (!SettingsPart.TrackOnAdmin && isAdmin)
                || (!SettingsPart.TrackOnFrontEnd && !isAdmin)) {
                return ""; // Not a valid configuration, ignore
            }

            // Tag manager deployment
            if (SettingsPart.UseTagManager) {
                return GoogleTagManagerScript(allowedTypes);
            }
            // analytics.js deployment
            return GoogleAnalyticsScript(allowedTypes);
        }

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

        private string GoogleAnalyticsScript(IList<CookieType> allowedTypes) {
            StringBuilder script = new StringBuilder(800);
            script.AppendLine("<!-- Google Analytics -->");
            script.AppendLine("<script async src='//www.google-analytics.com/analytics.js'></script>");
            script.AppendLine("<script>");
            script.AppendLine("window.ga=window.ga||function(){(ga.q=ga.q||[]).push(arguments)};ga.l=+new Date;");
            if (string.IsNullOrWhiteSpace(SettingsPart.DomainName)) {
                script.AppendLine("ga('create', '" + SettingsPart.GoogleAnalyticsKey + "', 'auto');");
            } else {
                script.AppendLine("ga('create', '" + SettingsPart.GoogleAnalyticsKey + "', {'cookieDomain': '" + SettingsPart.DomainName + "'});");
            }
            if (SettingsPart.AnonymizeIp || allowedTypes.Contains(CookieType.Statistical) == false) {
                script.AppendLine("ga('set', 'anonymizeIp', true);");
            }
            script.AppendLine("ga('send', 'pageview');");
            script.AppendLine("</script>");
            script.AppendLine("<!-- End Google Analytics -->");
            // Register Google's new, recommended asynchronous universal analytics script to the header
            return script.ToString();
        }

        private string GoogleTagManagerScript(IList<CookieType> allowedTypes) {
            var script = new StringBuilder();

            script.AppendLine("<!-- Google Tag Manager -->");
            script.AppendLine("<script type='text/javascript'>");
            script.AppendLine("window.dataLayer = window.dataLayer || [];");
            script.AppendLine("</script>");
            script.AppendLine("<script>(function(w,d,s,l,i){w[l]=w[l]||[];w[l].push({'gtm.start':");
            script.AppendLine("new Date().getTime(),event:'gtm.js'});var f=d.getElementsByTagName(s)[0],");
            script.AppendLine("j=d.createElement(s),dl=l!='dataLayer'?'&l='+l:'';j.async=true;j.src=");
            script.AppendLine("'//www.googletagmanager.com/gtm.js?id='+i+dl;f.parentNode.insertBefore(j,f);");
            script.AppendLine("})(window,document,'script','dataLayer','" + SettingsPart.GoogleAnalyticsKey + "');</script>");
            script.AppendLine("<!-- End Google Tag Manager -->");

            return script.ToString();
        }

        public string GetFootScript(IList<CookieType> allowedTypes) {
            return string.Empty;
        }

        public string GetNoScript() {
            var snippet = new StringBuilder();

            snippet.AppendLine("<!-- Google Tag Manager (noscript) -->");
            snippet.AppendLine("<noscript><iframe src='//www.googletagmanager.com/ns.html?id=" + SettingsPart.GoogleAnalyticsKey + "'");
            snippet.AppendLine("height='0' width='0' style='display: none; visibility: hidden'></iframe></noscript>");
            snippet.AppendLine("<!-- End Google Tag Manager (noscript) -->");

            return snippet.ToString();
        }
    }
}