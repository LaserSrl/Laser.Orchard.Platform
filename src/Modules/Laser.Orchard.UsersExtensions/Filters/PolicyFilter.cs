using Laser.Orchard.Commons.Attributes;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.UsersExtensions.Services;
using Orchard;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Filters;
using Orchard.OutputCache;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Laser.Orchard.UsersExtensions.Filters {
    public class PolicyFilter : FilterProvider, IActionFilter, ICachingEventHandler {

        private readonly IContentSerializationServices _contentSerializationServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPolicyServices _policyServices;
        private readonly IUsersExtensionsServices _userExtensionServices;
        private readonly IUtilsServices _utilsServices;
        private readonly IWorkContextAccessor _workContext;

        private string[] allowedControllers;
        private IEnumerable<int> _missingPolicies;

        public ILogger Logger { get; set; }

        public PolicyFilter(
            IContentSerializationServices contentSerializationServices,
            IHttpContextAccessor httpContextAccessor,
            IPolicyServices policyServices,
            IUsersExtensionsServices userExtensionServices,
            IUtilsServices utilsServices,
            IWorkContextAccessor workContext) {

            _contentSerializationServices = contentSerializationServices;
            _httpContextAccessor = httpContextAccessor;
            _policyServices = policyServices;
            _userExtensionServices = userExtensionServices;
            _utilsServices = utilsServices;
            _workContext = workContext;

            allowedControllers = new string[] {
                "Laser.Orchard.UsersExtensions.Controllers.UserActionsController",
                "Laser.Orchard.Policy.Controllers.PoliciesController",
                "Orchard.Users.Controllers.AccountController",
                "Laser.Orchard.OpenAuthentication.Controllers.AccountController",
                "Orchard.Taxonomies.Controllers.LocalizedTaxonomyController",
                "Nwazet.Commerce.Controllers.ShoppingCartController.NakedCart"
            };
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
            bool isAdminService = filterContext.ActionDescriptor.GetCustomAttributes(typeof(AdminServiceAttribute), false).Any();

            var fullActionName = filterContext.Controller.GetType().FullName + "." + filterContext.ActionDescriptor.ActionName;

            if (_workContext.GetContext().CurrentUser != null && 
                !allowedControllers.Contains(filterContext.Controller.GetType().FullName) && 
                !allowedControllers.Contains(fullActionName)  &&
                !AdminFilter.IsApplied(filterContext.RequestContext) &&
                !isAdminService) {
                var language = _workContext.GetContext().CurrentCulture;
                IEnumerable<PolicyTextInfoPart> neededPolicies = _userExtensionServices.GetUserLinkedPolicies(language);

                if (neededPolicies.Count() > 0) {
                    var missingPolicies = MissingRegistrationPolices();
                    if (missingPolicies.Count() > 0) {

                        if (filterContext.Controller.GetType().FullName == "Laser.Orchard.WebServices.Controllers.JsonController") {
                            string data = _policyServices.PoliciesLMNVSerialization(neededPolicies.Where(w => missingPolicies.Any(a => a == w.Id)));

                            filterContext.Result = new ContentResult { Content = data, ContentType = "application/json" };
                        }
                        else if (filterContext.Controller.GetType().FullName == "Laser.Orchard.WebServices.Controllers.WebApiController") {
                            string data = _policyServices.PoliciesPureJsonSerialization(neededPolicies.Where(w => missingPolicies.Any(a => a == w.Id)));

                            filterContext.Result = new ContentResult { Content = data, ContentType = "application/json" };
                        }
                        else {
                            string outputFormat = _workContext.GetContext().HttpContext.Request.Headers["OutputFormat"];

                            if (String.Equals(outputFormat, "LMNV", StringComparison.OrdinalIgnoreCase)) {
                                string data = _policyServices.PoliciesLMNVSerialization(neededPolicies.Where(w => missingPolicies.Any(a => a == w.Id)));
                                Response response = _utilsServices.GetResponse(ResponseType.MissingPolicies, "", Newtonsoft.Json.JsonConvert.DeserializeObject(data));

                                filterContext.Result = new ContentResult { Content = Newtonsoft.Json.JsonConvert.SerializeObject(response), ContentType = "application/json" };
                            }
                            else if (String.Equals(outputFormat, "PureJson", StringComparison.OrdinalIgnoreCase)) {
                                string data = _policyServices.PoliciesPureJsonSerialization(neededPolicies.Where(w => missingPolicies.Any(a => a == w.Id)));
                                Response response = _utilsServices.GetResponse(ResponseType.MissingPolicies, "", Newtonsoft.Json.JsonConvert.DeserializeObject(data));

                                filterContext.Result = new ContentResult { Content = Newtonsoft.Json.JsonConvert.SerializeObject(response), ContentType = "application/json" };
                            }
                            else {
                                var returnType = ((ReflectedActionDescriptor)filterContext.ActionDescriptor).MethodInfo.ReturnType;

                                if (returnType == typeof(JsonResult)) {
                                    string data = _policyServices.PoliciesPureJsonSerialization(neededPolicies.Where(w => missingPolicies.Any(a => a == w.Id)));
                                    Response response = _utilsServices.GetResponse(ResponseType.MissingPolicies, "", Newtonsoft.Json.JsonConvert.DeserializeObject(data));

                                    filterContext.Result = new ContentResult { Content = Newtonsoft.Json.JsonConvert.SerializeObject(response), ContentType = "application/json" };
                                }
                                else {
                                    var encodedAssociatedPolicies = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(",", missingPolicies)));

                                    UrlHelper urlHelper = new UrlHelper(_httpContextAccessor.Current().Request.RequestContext);
                                    var url = urlHelper.Action("Index", "Policies", new { area = "Laser.Orchard.Policy", lang = language, policies = encodedAssociatedPolicies, returnUrl = _httpContextAccessor.Current().Request.RawUrl });

                                    filterContext.Result = new RedirectResult(url);
                                }
                            }
                        }
                    }
                }
            }

            return;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) { }

        public void KeyGenerated(StringBuilder key) {
            var missingPolicies = MissingRegistrationPolices();
            if (missingPolicies != null && missingPolicies.Count() > 0)
                key.Append("pendingpolicies=" + String.Join("_", missingPolicies.Select(s => s)) + ";");
        }

        private IEnumerable<int> MissingRegistrationPolices() {
            if (_missingPolicies != null)
                return _missingPolicies;
            var language = _workContext.GetContext().CurrentCulture;
            // the following calls fetch information from the db. In the services where
            // they are implemented, they should be cached.
            IEnumerable<PolicyTextInfoPart> neededPolicies = _userExtensionServices
                .GetUserLinkedPolicies(language);
            var userPolicies = _policyServices
                .GetPoliciesForCurrentUser(false, language)
                .Policies
                .Where(w => w.Accepted || (w.AnswerDate > DateTime.MinValue && !w.PolicyText.UserHaveToAccept))
                .Select(s => s.PolicyTextId)
                .ToList();
            _missingPolicies = neededPolicies.Select(s => s.Id).ToList().Where(w => !userPolicies.Any(a => a == w));
            return _missingPolicies;
        }
    }
}