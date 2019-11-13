using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.StartupConfig.Services;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Mvc.Filters;
using Orchard.OutputCache;

namespace Laser.Orchard.Policy.Filters {

    public class PolicyFilter : FilterProvider, IActionFilter, IResultFilter, ICachingEventHandler {
        private readonly ICommonsServices _commonServices;
        private readonly IContentSerializationServices _contentSerializationServices;
        private readonly ICurrentContentAccessor _currenContent;
        private readonly IPolicyServices _policyServices;
        private IList<IContent> pendingPolicies;

        public PolicyFilter(ICommonsServices commonServices,
            IContentSerializationServices contentSerializationServices,
            ICurrentContentAccessor currenContent,
            IPolicyServices policyServices) {
            _commonServices = commonServices;
            _contentSerializationServices = contentSerializationServices;
            _currenContent = currenContent;
            _policyServices = policyServices;
        }

        public ILogger Logger;

        public void OnActionExecuting(ActionExecutingContext filterContext) {
            var routeData = HttpContext.Current.Request.RequestContext.RouteData;
            string areaName = (routeData.Values["area"] ?? string.Empty).ToString();
            string controllerName = (routeData.Values["controller"] ?? string.Empty).ToString();
            string actionName = (routeData.Values["action"] ?? string.Empty).ToString();

            if (areaName.Equals("Laser.Orchard.WebServices", StringComparison.InvariantCultureIgnoreCase) &&
                controllerName.Equals("WebApi", StringComparison.InvariantCultureIgnoreCase) &&
                actionName.Equals("display", StringComparison.InvariantCultureIgnoreCase)) {

                SetPendingPolicies();
                if (pendingPolicies != null && pendingPolicies.Count() > 0) {
                    JObject json;
                    json = new JObject();
                    var resultArray = new JArray();
                    foreach (var pendingPolicy in pendingPolicies) {
                        resultArray.Add(new JObject(_contentSerializationServices.SerializeContentItem((ContentItem)pendingPolicy, 0)));
                    }
                    json.Add("PendingPolicies", resultArray);
                    //_contentSerializationServices.NormalizeSingleProperty(json);
                    filterContext.Result = new ContentResult { Content = json.ToString(Newtonsoft.Json.Formatting.None), ContentType = "application/json" };
                    //return GetJson(content, page, pageSize);
                }
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
        }

        public void KeyGenerated(StringBuilder key) {

            SetPendingPolicies();

            if (pendingPolicies != null && pendingPolicies.Count() > 0)
                key.Append("pendingitempolicies=" + String.Join("_", pendingPolicies.Select(s => s.Id)) + ";");
        }

        private void SetPendingPolicies() {
            if (pendingPolicies != null)
                return;
            var routeData = HttpContext.Current.Request.RequestContext.RouteData;
            string areaName = (routeData.Values["area"] ?? string.Empty).ToString();
            string controllerName = (routeData.Values["controller"] ?? string.Empty).ToString();
            string actionName = (routeData.Values["action"] ?? string.Empty).ToString();
            string alias;
            IContent content;

            if (areaName.Equals("Laser.Orchard.WebServices", StringComparison.InvariantCultureIgnoreCase) &&
                controllerName.Equals("WebApi", StringComparison.InvariantCultureIgnoreCase) &&
                actionName.Equals("display", StringComparison.InvariantCultureIgnoreCase)) {
                // For our webapi, we determine the content through its alias
                alias = (HttpContext.Current.Request.Params["alias"] ?? string.Empty).ToString();
                content = _commonServices.GetContentByAlias(alias);
            }
            else if (areaName.Equals("Laser.Orchard.Policy", StringComparison.InvariantCultureIgnoreCase) &&
                      controllerName.Equals("Policies", StringComparison.InvariantCultureIgnoreCase) &&
                      actionName.Equals("Index", StringComparison.InvariantCultureIgnoreCase)) {
                // since we also cache the page where we ask to accept the policies,
                // and that's on a specific controller, we do need to figure out the content item
                // we are preventing the used to see
                alias = (HttpContext.Current.Request.Params["alias"] ?? string.Empty).ToString();
                content = _commonServices.GetContentByAlias(alias);
            }
            else {
                // this is the "normal" case, in which we are accessing a contentitem
                // directly on its display view
                content = _currenContent.CurrentContentItem;
            }
            //_maxLevel = maxLevel;
            if (content != null) {
                // content would be null if rather than on a content item we are trying
                // to go to a controller aciton. Those should manage their own permissions
                // and caching so they would not have to be handled here
                var policy = content.As<Models.PolicyPart>();
                if (policy != null && (_policyServices.HasPendingPolicies(content.ContentItem) ?? false)) { // Se l'oggetto ha delle pending policies allora devo scrivere la lista delle pending policies
                    pendingPolicies = _policyServices.PendingPolicies(content.ContentItem);
                }
                else {
                    pendingPolicies = new List<IContent>();
                }
            }
        }

    }
}


