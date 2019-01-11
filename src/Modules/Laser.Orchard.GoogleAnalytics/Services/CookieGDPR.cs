using Laser.Orchard.Cookies.Services;
using System.Collections.Generic;
using System.Web;
using Laser.Orchard.Cookies;
using System.Text;
using Orchard;
using Orchard.ContentManagement;
using OUI = Orchard.UI;
using Laser.Orchard.GoogleAnalytics.Models;

namespace Laser.Orchard.GoogleAnalytics.Services {
    public class CookieGDPR : ICookieGDPR {
        private readonly IOrchardServices _orchardServices;
        public CookieGDPR(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }
        public string GetCookieName() {
            return "Google Analytics";
        }

        public IList<CookieType> GetCookieTypes() {
            return new List<CookieType>() { CookieType.Technical, CookieType.Statistical };
        }

        public string GetScript(IList<CookieType> allowedTypes) {
            //Determine if we're on an admin page
            bool isAdmin = OUI.Admin.AdminFilter.IsApplied(HttpContext.Current.Request.RequestContext);

            //Get our part data/record if available for rendering scripts
            var part = _orchardServices.WorkContext.CurrentSite.As<GoogleAnalyticsSettingsPart>();
            if (part == null || string.IsNullOrWhiteSpace(part.GoogleAnalyticsKey) || (!part.TrackOnAdmin && isAdmin) || (!part.TrackOnFrontEnd && !isAdmin))
                return ""; // Not a valid configuration, ignore

            StringBuilder script = new StringBuilder(800);
            script.AppendLine("<!-- Google Analytics -->");
            script.AppendLine("<script async src='//www.google-analytics.com/analytics.js'></script>");
            script.AppendLine("<script>");
            script.AppendLine("window.ga=window.ga||function(){(ga.q=ga.q||[]).push(arguments)};ga.l=+new Date;");
            if (string.IsNullOrWhiteSpace(part.DomainName)) {
                script.AppendLine("ga('create', '" + part.GoogleAnalyticsKey + "', 'auto');");
            } else {
                script.AppendLine("ga('create', '" + part.GoogleAnalyticsKey + "', {'cookieDomain': '" + part.DomainName + "'});");
            }
            if (part.AnonymizeIp || allowedTypes.Contains(CookieType.Statistical) == false) {
                script.AppendLine("ga('set', 'anonymizeIp', true);");
            }
            script.AppendLine("ga('send', 'pageview');");
            script.AppendLine("</script>");
            script.AppendLine("<!-- End Google Analytics -->");
            // Register Google's new, recommended asynchronous universal analytics script to the header
            return script.ToString();
        }
    }
}