using System.Text;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Mvc;
using Orchard.Mvc.Filters;
using Orchard.UI.Resources;
using Laser.Orchard.GoogleAnalytics.Models;
using OUI = Orchard.UI;


namespace Laser.Orchard.GoogleAnalytics.Filters {
    [OrchardFeature("Laser.Orchard.GoogleAnalytics")]
    public class GoogleAnalyticsFilter : FilterProvider, IResultFilter {
        private readonly IResourceManager _resourceManager;
        private readonly IOrchardServices _orchardServices;

        public GoogleAnalyticsFilter(IResourceManager resourceManager, IOrchardServices orchardServices) {
            _resourceManager = resourceManager;
            _orchardServices = orchardServices;
        }

        #region IResultFilter Members

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult == null)
                return;

            //Determine if we're on an admin page
            bool isAdmin = OUI.Admin.AdminFilter.IsApplied(filterContext.RequestContext);

            //Get our part data/record if available for rendering scripts
            var part = _orchardServices.WorkContext.CurrentSite.As<GoogleAnalyticsSettingsPart>();
            if (part == null || string.IsNullOrWhiteSpace(part.GoogleAnalyticsKey) || (!part.TrackOnAdmin && isAdmin) || (!part.TrackOnFrontEnd && !isAdmin))
                return; // Not a valid configuration, ignore filter

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
            if (part.AnonymizeIp) {
                script.AppendLine("ga('set', 'anonymizeIp', true);");
            }
            script.AppendLine("ga('send', 'pageview');");
            script.AppendLine("</script>");
            script.AppendLine("<!-- End Google Analytics -->");
            // Register Google's new, recommended asynchronous universal analytics script to the header
            _resourceManager.RegisterHeadScript(script.ToString());
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }

        #endregion
    }
}