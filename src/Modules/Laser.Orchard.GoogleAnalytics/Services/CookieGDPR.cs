using Laser.Orchard.Cookies;
using Laser.Orchard.Cookies.Services;
using Laser.Orchard.GoogleAnalytics.Models;
using Laser.Orchard.GoogleAnalytics.ViewModels;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using OUI = Orchard.UI;

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
            bool isAdmin = OUI.Admin.AdminFilter.IsApplied(HttpContext.Current.Request.RequestContext);
            bool addGTM = false;

            if (SettingsPart != null) {
                addGTM = (!string.IsNullOrWhiteSpace(SettingsPart.GTMContainerId) &&
                      ((SettingsPart.TrackGTMOnAdmin && isAdmin) ||
                      (SettingsPart.TrackGTMOnFrontEnd && !isAdmin)));
            }

            if (SettingsPart != null && addGTM) {
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
            var finalScript = string.Empty;

            //Determine if we're on an admin page
            bool isAdmin = OUI.Admin.AdminFilter.IsApplied(HttpContext.Current.Request.RequestContext);

            bool addScript = (SettingsPart != null);

            if (addScript) {
                bool addGTM = (!string.IsNullOrWhiteSpace(SettingsPart.GTMContainerId) &&
                    ((SettingsPart.TrackGTMOnAdmin && isAdmin) ||
                    (SettingsPart.TrackGTMOnFrontEnd && !isAdmin)));

                // Tag manager deployment
                if (addGTM) {
                    finalScript += GoogleTagManagerScript(allowedTypes);
                } else {
                    finalScript += GetNoGTMScript();
                }

                bool addAnalytics = (!string.IsNullOrWhiteSpace(SettingsPart.GoogleAnalyticsKey) &&
                    ((SettingsPart.TrackOnAdmin && isAdmin) ||
                    (SettingsPart.TrackOnFrontEnd && !isAdmin)));

                // analytics.js deployment
                if (addAnalytics) {
                    finalScript += GoogleAnalyticsScript(allowedTypes);
                } else {
                    finalScript += GetNoAnalyticsScript();
                }
            }
            return finalScript;
        }

        private string GetNoGTMScript() {
            StringBuilder script = new StringBuilder();
            script.AppendLine("<script>");
            script.AppendLine("window.useGTM = 0;");
            script.AppendLine("</script>");
            return script.ToString();
        }

        private string GetNoAnalyticsScript() {
            StringBuilder script = new StringBuilder();
            script.AppendLine("<script>");
            script.AppendLine("window.useGA4 = 0;");
            script.AppendLine("window.useUA = 0;");
            script.AppendLine("</script>");
            return script.ToString();
        }

        private GASettingsVM SettingsPart {
            get {
                return _cacheManager.Get(Constants.SiteSettingsCacheKey, true, ctx => {
                    // check whether we should invalidate the cache
                    ctx.Monitor(_signals.When(Constants.SiteSettingsEvictSignal));
                    return new GASettingsVM(_workContextAccessor
                        .GetContext()
                        .CurrentSite
                        .As<GoogleAnalyticsSettingsPart>());
                });
            }
        }

        private string HostDomain() {
            var valueToReplace = "www.";
            var host = "";

            if (!_workContextAccessor.GetContext().HttpContext.Request.IsLocal) {
                host = _workContextAccessor.GetContext().HttpContext.Request.Url.Host;
                if (host.Substring(0, 4) == valueToReplace) {
                    host = host.Substring(4, host.Length - 4);
                }
            }
            return host;
        }

        private string GoogleAnalyticsScript(IList<CookieType> allowedTypes) {
            bool isAdmin = OUI.Admin.AdminFilter.IsApplied(HttpContext.Current.Request.RequestContext);
            var gaSettings = _workContextAccessor.GetContext().CurrentSite.As<GoogleAnalyticsSettingsPart>();
            bool useGTM = (!string.IsNullOrWhiteSpace(SettingsPart.GTMContainerId) &&
                    ((SettingsPart.TrackGTMOnAdmin && isAdmin) ||
                    (SettingsPart.TrackGTMOnFrontEnd && !isAdmin)));

            // If Tag Manager is enabled, I don't need to add anything here.
            // If Google Analytics Key is in the format UA-XXXXXXX-X, old Universal Analytics is used.
            // If it's in the format G-YYYYYYY, GA4 is used.
            var ua = gaSettings.GoogleAnalyticsKey.StartsWith("UA-");
            var ga4 = gaSettings.GoogleAnalyticsKey.StartsWith("G-");
            if (ga4) {
                StringBuilder script = new StringBuilder();
                script.AppendLine("<script async src=\"https://www.googletagmanager.com/gtag/js?id=" + gaSettings.GoogleAnalyticsKey + "\"></script>");
                script.AppendLine("<script>");
                script.AppendLine("window.useGA4 = 1;");
                script.AppendLine("window.useUA = 0;");

                // Add gtag javascript function to the script.
                script.AppendLine("window.dataLayer = window.dataLayer || [];");
                if (!useGTM) {
                    // gtag() function is already added by GTM script, so we add it again only if GTM isn't enabled.
                    script.AppendLine("function gtag() {");
                    script.AppendLine("    window.dataLayer = window.dataLayer || [];");
                    script.AppendLine("    dataLayer.push(arguments);");
                    script.AppendLine("}");
                }
                script.AppendLine("gtag('js', new Date());");
                script.AppendLine("gtag('config', '" + gaSettings.GoogleAnalyticsKey + "');");
                // End gtag script

                script.AppendLine("</script>");

                return script.ToString();
            } else if (!useGTM && ua) {
                StringBuilder script = new StringBuilder();
                script.AppendLine("<!-- Google Analytics -->");
                script.AppendLine("<script async src='//www.google-analytics.com/analytics.js'></script>");
                script.AppendLine("<script>");
                script.AppendLine("window.useGA4 = 0;");
                script.AppendLine("window.useUA = 1;");
                script.AppendLine("window.ga=window.ga||function(){(ga.q=ga.q||[]).push(arguments)};ga.l=+new Date;");

                script.AppendLine("ga('create', '" + SettingsPart.GoogleAnalyticsKey + "', {");
                if (string.IsNullOrWhiteSpace(SettingsPart.DomainName)) {
                    script.AppendLine("'cookieDomain': '" + HostDomain() + "',");
                } else {
                    script.AppendLine("'cookieDomain': '" + SettingsPart.DomainName + "',");
                }
                if (!allowedTypes.Contains(CookieType.Statistical)) {
                    script.AppendLine("'storage': 'none',");
                    script.AppendLine("storeGac: false,");
                }
                script.AppendLine("});");

                if (SettingsPart.AnonymizeIp || allowedTypes.Contains(CookieType.Statistical) == false) {
                    script.AppendLine("ga('set', 'anonymizeIp', true);");
                }
                if (allowedTypes.Contains(CookieType.Statistical)) {
                    script.AppendLine("ga('send', 'pageview');");
                }
                script.AppendLine("</script>");
                script.AppendLine("<!-- End Google Analytics -->");
                // Register Google's new, recommended asynchronous universal analytics script to the header
                return script.ToString();
            } else {
                StringBuilder script = new StringBuilder();
                script.AppendLine("<script>");
                script.AppendLine("window.useGA4 = 0;");
                if (ua) {
                    script.AppendLine("window.useUA = 1;");
                }
                script.AppendLine("</script>");
                return script.ToString();
            }

            return "";
        }

        private string GoogleTagManagerScript(IList<CookieType> allowedTypes) {
            var script = new StringBuilder();

            script.AppendLine("<!-- Google Tag Manager -->");
            script.AppendLine("<script type='text/javascript'>");
            script.Append("window.useGTM = 1;");
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
            // set the default value of cookie domain
            if (string.IsNullOrWhiteSpace(SettingsPart.DomainName)) {
                script.AppendLine("window.dataLayer.push({'DefaultCookieDomain': '"
                    + HostDomain() + "'});");
            } else {
                script.AppendLine("window.dataLayer.push({'DefaultCookieDomain': '"
                    + SettingsPart.DomainName + "'});");
            }
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
            // tag manager consent settings
            script.AppendLine("window.dataLayer.push(");
            script.AppendLine("    'consent', 'default', {");
            script.AppendLine("        'ad_storage': 'denied',");
            script.AppendLine("        'functionality_storage': 'denied',");
            script.AppendLine("        'security_storage': 'granted',");
            script.AppendLine("        'personalization_storage': 'denied',");
            script.AppendLine("        'analytics_storage': 'denied'");
            script.AppendLine("	    });");
            if (allowedTypes.Contains(CookieType.Statistical)
                || allowedTypes.Contains(CookieType.Marketing)
                || allowedTypes.Contains(CookieType.Preferences)) {
                script.AppendLine("window.dataLayer.push('consent', 'update', {");
                if (allowedTypes.Contains(CookieType.Marketing)) {
                    script.AppendLine("    'ad_storage': 'granted',");
                }
                if (allowedTypes.Contains(CookieType.Preferences)) {
                    script.AppendLine("    'personalization_storage': 'granted',");
                }
                if (allowedTypes.Contains(CookieType.Statistical)) {
                    script.AppendLine("    'analytics_storage': 'granted'");
                }
                script.AppendLine("	});");
            }

            // done tag manager consent settings
            script.AppendLine("</script>");

            // gtag() function
            script.AppendLine("<script>");
            script.AppendLine("function gtag() {");
            script.AppendLine("    window.dataLayer = window.dataLayer || [];");
            script.AppendLine("    dataLayer.push(arguments);");
            script.AppendLine("}");
            script.AppendLine("</script>");

            script.AppendLine("<script>(function(w,d,s,l,i){w[l]=w[l]||[];w[l].push({'gtm.start':");
            script.AppendLine("new Date().getTime(),event:'gtm.js'});var f=d.getElementsByTagName(s)[0],");
            script.AppendLine("j=d.createElement(s),dl=l!='dataLayer'?'&l='+l:'';j.async=true;j.src=");
            script.AppendLine("'//www.googletagmanager.com/gtm.js?id='+i+dl;f.parentNode.insertBefore(j,f);");
            script.AppendLine("})(window,document,'script','dataLayer','" + SettingsPart.GTMContainerId + "');</script>");

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

            bool useGTM = (!string.IsNullOrWhiteSpace(SettingsPart.GTMContainerId) &&
                    ((SettingsPart.TrackGTMOnAdmin && isAdmin) ||
                    (SettingsPart.TrackGTMOnFrontEnd && !isAdmin)));

            // Tag manager deployment
            if (useGTM) {

                var snippet = new StringBuilder();

                snippet.AppendLine("<!-- Google Tag Manager (noscript) -->");
                snippet.AppendLine("<noscript><iframe src='//www.googletagmanager.com/ns.html?id=" + SettingsPart.GTMContainerId + "'");
                snippet.AppendLine("height='0' width='0' style='display: none; visibility: hidden'></iframe></noscript>");
                snippet.AppendLine("<!-- End Google Tag Manager (noscript) -->");

                return snippet.ToString();
            }

            return string.Empty;
        }
    }
}