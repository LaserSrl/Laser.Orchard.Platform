using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
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
        private IList<IContent> pendingPolicies;

        public PolicyFilter(ICommonsServices commonServices,
            IContentSerializationServices contentSerializationServices) {
            _commonServices = commonServices;
            _contentSerializationServices = contentSerializationServices;
        }

        public ILogger Logger;

        public void OnActionExecuting(ActionExecutingContext filterContext) {

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

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
        }

        public void KeyGenerated(StringBuilder key) {

            SetPendingPolicies();

            if (pendingPolicies != null && pendingPolicies.Count() > 0)
                key.Append("pendingpolicies=" + String.Join("_", pendingPolicies.Select(s => s.Id)) + ";");
        }

        private void SetPendingPolicies() {
            if (pendingPolicies != null)
                return;
            var routeData = HttpContext.Current.Request.RequestContext.RouteData;
            string areaName = (routeData.Values["area"] ?? string.Empty).ToString();
            string controllerName = (routeData.Values["controller"] ?? string.Empty).ToString();
            string actionName = (routeData.Values["action"] ?? string.Empty).ToString();
            if (areaName.Equals("Laser.Orchard.WebServices", StringComparison.InvariantCultureIgnoreCase) &&
                controllerName.Equals("WebApi", StringComparison.InvariantCultureIgnoreCase) &&
                actionName.Equals("display", StringComparison.InvariantCultureIgnoreCase)) {
                string alias = (HttpContext.Current.Request.Params["alias"] ?? string.Empty).ToString();

                var content = _commonServices.GetContentByAlias(alias);
                //_maxLevel = maxLevel;
                if (content != null) {
                    var policy = content.As<Models.PolicyPart>();
                    if (policy != null && (policy.HasPendingPolicies ?? false)) { // Se l'oggetto ha delle pending policies allora devo serivre la lista delle pending policies
                        pendingPolicies = policy.PendingPolicies;
                    } else {
                        pendingPolicies = new List<IContent>();
                    }
                }
            }
        }

    }

}


