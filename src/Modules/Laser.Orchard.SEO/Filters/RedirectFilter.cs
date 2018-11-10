using Laser.Orchard.SEO.Services;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Filters;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.SEO.Filters {
    [OrchardFeature("Laser.Orchard.Redirects")]
    public class RedirectFilter : FilterProvider, IActionFilter {
        private readonly IRedirectService _redirectService;
        private readonly ShellSettings _shellSettings;
        private readonly IWorkContextAccessor _wca;

        public RedirectFilter(
            IRedirectService redirectService,
            ShellSettings shellSettings,
            IWorkContextAccessor wca) {

            _redirectService = redirectService;
            _shellSettings = shellSettings;
            _wca = wca;
        }
        
        public void OnActionExecuting(ActionExecutingContext filterContext) {
            var url = filterContext.RequestContext.HttpContext.Request.Url;
            if (url == null)
                return;

            if (AdminFilter.IsApplied(new RequestContext(filterContext.HttpContext, new RouteData()))) {
                return; //no automated redirects from admin
            }
            if (filterContext.HttpContext.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase)) {
                return; //do not redirect POST requests
            }

            var urlPrefix = _shellSettings.RequestUrlPrefix;
            var applicationPath = filterContext.HttpContext.Request.ApplicationPath;
            var strippedSegments = url.Segments.Select(s => s.Trim('/')).ToList();
            //to generate the path we want to use, remove from the segments the ApplicationPath and UrlPrefix
            //remove only the first of each, because there may be segments with the same "value"
            //e.g. in https://localhost/Laser.Orchard/LaserTest/LaserTest:
            // ApplicationPath == "/Laser.Orchard"
            // UrlPrefix == "LaserTest"
            // the second LaserTest is the alias of an actual page.
            strippedSegments
                .Remove(strippedSegments
                    .FirstOrDefault(s => s.Equals(applicationPath.Trim('/'), StringComparison.InvariantCultureIgnoreCase)));
            if (!string.IsNullOrWhiteSpace(urlPrefix)) {
                strippedSegments
                    .Remove(strippedSegments
                        .FirstOrDefault(s => s.Equals(urlPrefix.Trim('/'), StringComparison.InvariantCultureIgnoreCase)));
            }

            var path = string.Join("/", strippedSegments);
            var redirect = _redirectService.GetRedirect(path);

            if (redirect == null)
                return;

            
            var destination = _wca.GetContext().CurrentSite.BaseUrl + //not a fan of this, since BaseUrl can be edited by admin
                (string.IsNullOrWhiteSpace(urlPrefix) ? "" : "/" + urlPrefix) +
                "/" + redirect.DestinationUrl.TrimStart('/');
            filterContext.Result = new RedirectResult(destination + url.Query, redirect.IsPermanent);
        }
        
        public void OnActionExecuted(ActionExecutedContext filterContext) { }
    }
}