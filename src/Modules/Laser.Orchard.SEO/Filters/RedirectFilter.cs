using Laser.Orchard.SEO.Services;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Mvc.Filters;
using Orchard.UI.Admin;
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.SEO.Filters {
    [OrchardFeature("Laser.Orchard.Redirects")]
    public class RedirectFilter : FilterProvider, IActionFilter {
        private readonly IRedirectService _redirectService;
        private readonly ShellSettings _shellSettings;
        private readonly IWorkContextAccessor _wca;

        public ILogger Log { get; set; }

        public RedirectFilter(
            IRedirectService redirectService,
            ShellSettings shellSettings,
            IWorkContextAccessor wca) {

            _redirectService = redirectService;
            _shellSettings = shellSettings;
            _wca = wca;
            Log = NullLogger.Instance;
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
            strippedSegments.RemoveAll(s => string.IsNullOrEmpty(s));
            var normalizedApplicationPath = applicationPath.TrimEnd('/');
            var serverUrl = GetServerUrl(url);

            //if querystring is in redirects table, use it
            var pathQs = string.Join("/", strippedSegments) + url.Query;
            var redirect = _redirectService.GetCachedRedirects().FirstOrDefault(x => x.SourceUrl == pathQs);
            if (redirect == null) {
                // else strip querystring to look for a match
                var path = string.Join("/", strippedSegments);
                redirect = _redirectService.GetCachedRedirects().FirstOrDefault(x => x.SourceUrl == path);
                if (redirect != null) {
                    var destination = serverUrl + normalizedApplicationPath +
                        (string.IsNullOrWhiteSpace(urlPrefix) ? "" : "/" + urlPrefix) +
                        "/" + redirect.DestinationUrl.TrimStart('/');
                    filterContext.Result = new RedirectResult(destination + url.Query, redirect.IsPermanent);
                }
            }
            else {
                var destination = serverUrl + normalizedApplicationPath +
                    (string.IsNullOrWhiteSpace(urlPrefix) ? "" : "/" + urlPrefix) +
                    "/" + redirect.DestinationUrl.TrimStart('/');
                filterContext.Result = new RedirectResult(destination, redirect.IsPermanent);
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) { }

        private string GetServerUrl(Uri url) {
            StringBuilder sb = new StringBuilder();
            int slashCounter = 0;
            foreach (var c in url.AbsoluteUri.ToCharArray()) {
                if (c == '/') {
                    slashCounter++;
                }
                if (slashCounter > 2) {
                    break;
                }
                else {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}