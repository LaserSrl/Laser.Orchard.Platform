using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.StartupConfig.Services;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Mvc.Filters;
using Orchard.OutputCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.Policy.Filters {

    public class PolicyFilter : FilterProvider, IActionFilter, IResultFilter, ICachingEventHandler {
        private readonly ICommonsServices _commonServices;
        private readonly IContentSerializationServices _contentSerializationServices;
        private readonly ICurrentContentAccessor _currenContent;
        private readonly IPolicyServices _policyServices;
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        private IList<IContent> pendingPolicies;

        public PolicyFilter(ICommonsServices commonServices,
            IContentSerializationServices contentSerializationServices,
            ICurrentContentAccessor currenContent,
            IPolicyServices policyServices,
            IContentManager contentManager,
            IWorkContextAccessor workContextAccessor) {

            _commonServices = commonServices;
            _contentSerializationServices = contentSerializationServices;
            _currenContent = currenContent;
            _policyServices = policyServices;
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
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
                        resultArray.Add(_contentSerializationServices.GetJson((ContentItem)pendingPolicy, 0));
                    }
                    json.Add("PendingPolicies", resultArray);
                    //_contentSerializationServices.NormalizeSingleProperty(json);
                    filterContext.Result = new ContentResult { Content = json.ToString(Newtonsoft.Json.Formatting.None), ContentType = "application/json" };
                    //return GetJson(content, page, pageSize);
                }
            } else if (areaName.Equals("Orchard.CustomForms", StringComparison.InvariantCultureIgnoreCase) &&
                controllerName.Equals("Item", StringComparison.InvariantCultureIgnoreCase) &&
                actionName.Equals("Create", StringComparison.InvariantCultureIgnoreCase)) {
                // When we are trying to "protect" access to a CustomForm, we need to do it through a filter
                // because displaying it goes through its own controller rather than the Display Action.
                // Get the content correspnding to the CustomForm
                int contentId;
                if (int.TryParse(routeData.Values["id"]?.ToString(), out contentId)) {
                    var content = _contentManager.Get(contentId);
                    if (content != null) {
                        var policyPart = content.As<PolicyPart>();
                        if (policyPart != null 
                            && (_policyServices.HasPendingPolicies(content) ?? false)) {
                            // Replicates the logic we have in PolicyPartDriver to redirect the user
                            // to the action to accept the required policies
                            var language = _workContextAccessor.GetContext().CurrentCulture;

                            var associatedPolicies = _policyServices.GetPoliciesForContent(policyPart);
                            var encodedAssociatedPolicies = "";
                            if (associatedPolicies != null)
                                encodedAssociatedPolicies = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(",", associatedPolicies)));
                            else
                                encodedAssociatedPolicies = Convert.ToBase64String(Encoding.UTF8.GetBytes(""));

                            var cookie = HttpContext.Current.Request.Cookies["PoliciesAnswers"];
                            if (cookie != null && cookie.Value != null) {
                                HttpContext.Current.Response.Cookies.Add(HttpContext.Current.Request.Cookies["PoliciesAnswers"]);
                            }
                            var rvd = new RouteValueDictionary();
                            rvd["area"] = "Laser.Orchard.Policy";
                            rvd["controller"] = "Policies";
                            rvd["action"] = "Index";
                            rvd["lang"] = language;
                            rvd["policies"] = encodedAssociatedPolicies;
                            rvd["returnUrl"] = HttpContext.Current.Request.RawUrl;
                            var autoroutePart = policyPart.As<AutoroutePart>();
                            if (autoroutePart != null && !string.IsNullOrWhiteSpace(autoroutePart.DisplayAlias)) {
                                rvd["alias"] = autoroutePart.DisplayAlias;
                            }

                            filterContext.Result = new RedirectToRouteResult(rvd);
                        }
                    }
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

            var policyPart = CurrentContent()?.As<PolicyPart>();
            if (policyPart != null) {
                // get the ids of pending policies
                var pending = _policyServices.GetPendingPolicyIds(policyPart);
                if (pending!= null && pending.Any()) {
                    key.Append(
                        "policy-not-accepted;pendingitempolicies=" 
                        + string.Join("_", pending) + ";");
                } else {
                    key.Append("policy-accepted;");
                }
            }
        }
        /*
         This Key generated used to be in the PolicyPartDriver. It is reproduced here because
         its logic was replicating what is already done here. However it was adding some potentially
         useful bits to the cachekey, so those are moved here.
         
         public void KeyGenerated(StringBuilder key) {
            var part = _currentContentAccessor.CurrentContentItem.As<PolicyPart>();
            if (part == null) return;

            if (_policyServices.HasPendingPolicies(part.ContentItem) ?? false) {
                _additionalCacheKey = "policy-not-accepted;";
                _additionalCacheKey += "pendingitempolicies=" + String.Join("_", _policyServices.PendingPolicies(part.ContentItem).Select(s => s.Id)) + ";";
            }
            else {
                _additionalCacheKey = "policy-accepted;";
            }

            key.Append(_additionalCacheKey);
        }
             */

        private void SetPendingPolicies() {
            if (pendingPolicies != null)
                return;

            IContent content = CurrentContent();

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

        private IContent CurrentContent() {
            var routeData = HttpContext.Current.Request.RequestContext.RouteData;
            string areaName = (routeData.Values["area"] ?? string.Empty).ToString();
            string controllerName = (routeData.Values["controller"] ?? string.Empty).ToString();
            string actionName = (routeData.Values["action"] ?? string.Empty).ToString();
            string alias;
            IContent content = null;

            if (areaName.Equals("Laser.Orchard.WebServices", StringComparison.InvariantCultureIgnoreCase) &&
                controllerName.Equals("WebApi", StringComparison.InvariantCultureIgnoreCase) &&
                actionName.Equals("display", StringComparison.InvariantCultureIgnoreCase)) {
                // For our webapi, we determine the content through its alias
                alias = (HttpContext.Current.Request.Params["alias"] ?? string.Empty).ToString();
                content = _commonServices.GetContentByAlias(alias);
            } else if (areaName.Equals("Laser.Orchard.Policy", StringComparison.InvariantCultureIgnoreCase) &&
                        controllerName.Equals("Policies", StringComparison.InvariantCultureIgnoreCase) &&
                        actionName.Equals("Index", StringComparison.InvariantCultureIgnoreCase)) {
                // since we also cache the page where we ask to accept the policies,
                // and that's on a specific controller, we do need to figure out the content item
                // we are preventing the used to see
                alias = (HttpContext.Current.Request.Params["alias"] ?? string.Empty).ToString();
                content = _commonServices.GetContentByAlias(alias);
            } else {
                // this is the "normal" case, in which we are accessing a contentitem
                // directly on its display view
                content = _currenContent.CurrentContentItem;
            }
            return content;
        }

    }
}


