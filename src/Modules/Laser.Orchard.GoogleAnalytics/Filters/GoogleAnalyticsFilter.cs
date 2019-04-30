using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Filters;
using Orchard.UI.Resources;
using Laser.Orchard.GoogleAnalytics.Models;
using OUI = Orchard.UI;
using Laser.Orchard.GoogleAnalytics.Services;

namespace Laser.Orchard.GoogleAnalytics.Filters {
    [OrchardFeature("Laser.Orchard.GoogleAnalytics")]
    public class GoogleAnalyticsFilter : FilterProvider, IResultFilter {
        private readonly IResourceManager _resourceManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IGoogleAnalyticsCookie _googleAnalyticsCookie;

        public GoogleAnalyticsFilter(IResourceManager resourceManager, IOrchardServices orchardServices, IGoogleAnalyticsCookie googleAnalyticsCookie) {
            _resourceManager = resourceManager;
            _orchardServices = orchardServices;
            _googleAnalyticsCookie = googleAnalyticsCookie;
        }

        #region IResultFilter Members

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult == null)
                return;

            //Determine if we're on an admin page
            bool isAdmin = OUI.Admin.AdminFilter.IsApplied(filterContext.RequestContext);
            var part = _orchardServices.WorkContext.CurrentSite.As<GoogleAnalyticsSettingsPart>();
            if (part != null && part.TrackOnAdmin && isAdmin && string.IsNullOrWhiteSpace(part.GoogleAnalyticsKey) == false) {
                // Register Google's new, recommended asynchronous universal analytics script to the header
                _resourceManager.RegisterHeadScript(_googleAnalyticsCookie.GetScript(_googleAnalyticsCookie.GetCookieTypes()));
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }

        #endregion
    }
}