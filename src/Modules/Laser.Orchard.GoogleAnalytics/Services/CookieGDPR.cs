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
            if (SettingsPart != null
                && !string.IsNullOrWhiteSpace(SettingsPart.GoogleAnalyticsKey)
                && SettingsPart.UseTagManager) {
                // We need to return all cookie types for tag manager, because 
                // addition of some cookies may be managed by tags, and they 
                // will need to be told whether any type is refused.
                return new List<CookieType>() {
                    CookieType.Technical, CookieType.Statistical,
                    CookieType.Preferences, CookieType.Marketing};
            }
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
            if (SettingsPart.AnonymizeIp || !allowedTypes.Contains(CookieType.Statistical)) {
                // insert into the datalayer a variable that tells to anonymize
                // ips for gathered interactions (i.e. fired tags)
                script.AppendLine("window.dataLayer.push({'anonymizeIp': 'true'});");
            }
            // set the initial (on page load) values of cookie consents
            script.AppendLine("window.dataLayer.push({'preferencesCookiesAccepted': '" 
                + allowedTypes.Contains(CookieType.Preferences).ToString().ToLowerInvariant() + "'});");
            script.AppendLine("window.dataLayer.push({'statisticalCookiesAccepted': '"
                + allowedTypes.Contains(CookieType.Statistical).ToString().ToLowerInvariant() + "'});");
            script.AppendLine("window.dataLayer.push({'marketingCookiesAccepted': '"
                + allowedTypes.Contains(CookieType.Marketing).ToString().ToLowerInvariant() + "'});");
            // script that handles changes in the settings for cookie consent
            script.AppendLine("$(document)");
            script.AppendLine("	.on('cookieConsent.reset', function(e) {");
            script.AppendLine("		window.dataLayer.push({");
            script.AppendLine("			'event': 'cookieConsent',");
            script.AppendLine("			'preferencesCookiesAccepted': false,");
            script.AppendLine("			'statisticalCookiesAccepted': false,");
            script.AppendLine("			'marketingCookiesAccepted': false");
            script.AppendLine("		});");
            script.AppendLine("	})");
            script.AppendLine("	.on('cookieConsent.accept', function(e, options) {");
            script.AppendLine("		window.dataLayer.push({");
            script.AppendLine("			'event': 'cookieConsent',");
            script.AppendLine("			'preferencesCookiesAccepted': options.preferences,");
            script.AppendLine("			'statisticalCookiesAccepted': options.statistical,");
            script.AppendLine("			'marketingCookiesAccepted': options.marketing");
            script.AppendLine("		});");
            script.AppendLine("	});");
            // done handlers for changes in cookie consent
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
            //Determine if we're on an admin page
            bool isAdmin = OUI.Admin.AdminFilter.IsApplied(HttpContext.Current.Request.RequestContext);

            //Get our part data/record if available for rendering scripts
            if (SettingsPart == null
                || string.IsNullOrWhiteSpace(SettingsPart.GoogleAnalyticsKey)
                || (!SettingsPart.TrackOnAdmin && isAdmin)
                || (!SettingsPart.TrackOnFrontEnd && !isAdmin)) {
                return string.Empty; // Not a valid configuration, ignore
            }

            // Tag manager deployment
            if (SettingsPart.UseTagManager) {

                var snippet = new StringBuilder();

                snippet.AppendLine("<!-- Google Tag Manager (noscript) -->");
                snippet.AppendLine("<noscript><iframe src='//www.googletagmanager.com/ns.html?id=" + SettingsPart.GoogleAnalyticsKey + "'");
                snippet.AppendLine("height='0' width='0' style='display: none; visibility: hidden'></iframe></noscript>");
                snippet.AppendLine("<!-- End Google Tag Manager (noscript) -->");

                return snippet.ToString();
            }

            return string.Empty;
        }
    }
}