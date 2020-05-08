using Laser.Orchard.Commons.Attributes;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.UsersExtensions.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Laser.Orchard.UsersExtensions.Filters {
    /// <summary>
    /// This action filter prevents an action from being executed if the calling user
    /// has not accepted the required policies
    /// </summary>
    public class PolicyApiFilterAttribute : ActionFilterAttribute {

        IDictionary<string, Func<IEnumerable<PolicyTextInfoPart>, IPolicyServices, string>> ResponseDictionary;

        public PolicyApiFilterAttribute() {
            ResponseDictionary = new Dictionary<string, Func<IEnumerable<PolicyTextInfoPart>, IPolicyServices, string>>() {
                { "LMNV", (parts, service) => service.PoliciesLMNVSerialization(parts) }, //keys should be all caps
                { "PUREJSON", (parts, service) => service.PoliciesPureJsonSerialization(parts) }
            };
        }

        public override void OnActionExecuting(HttpActionContext actionContext) {
            var isAdminService = actionContext
                .ActionDescriptor
                .GetCustomAttributes<AdminServiceAttribute>(false)
                .Any();

            var _workContext = actionContext.ControllerContext.GetWorkContext();
            var _userExtensionServices = _workContext.Resolve<IUsersExtensionsServices>();
            var _policyService = _workContext.Resolve<IPolicyServices>();
            var _utilsServices = _workContext.Resolve<IUtilsServices>();
            var currentUser = _workContext.CurrentUser;

            if (currentUser != null &&
                _userExtensionServices != null &&
                _policyService != null &&
                !isAdminService) {

                var language = _workContext.CurrentCulture;
                var neededPolicies = _userExtensionServices
                    .GetUserLinkedPolicies(language);

                if (neededPolicies.Any()) {
                    var userPolicyIds = _policyService
                        .GetPoliciesForCurrentUser(false, language)
                        .Policies
                        .Where(po =>
                            po.Accepted ||
                            (po.AnswerDate > DateTime.MinValue && !po.PolicyText.UserHaveToAccept))
                        .Select(po => po.PolicyTextId);
                    var missingPolicyIds = neededPolicies
                        .Select(po => po.Id)
                        .Except(userPolicyIds);

                    if (missingPolicyIds.Any()) {
                        var outputFormat = _workContext.HttpContext.Request
                            .Headers["OutputFormat"];
                        outputFormat = string.IsNullOrWhiteSpace(outputFormat) ? "LMNV" : outputFormat.ToUpperInvariant(); //capitalize OutputFOrmat

                        var requiredPolicies = neededPolicies
                            .Where(po => missingPolicyIds.Contains(po.Id));
                        Response response;
                        Func<IEnumerable<PolicyTextInfoPart>, IPolicyServices, string> dataFunc;
                        if (ResponseDictionary.TryGetValue(outputFormat, out dataFunc)) {
                            response = _utilsServices
                                .GetResponse(ResponseType.MissingPolicies, "",
                                    JsonConvert.DeserializeObject(dataFunc(requiredPolicies, _policyService)));
                        } else if (actionContext.ActionDescriptor.ReturnType == typeof(System.Web.Mvc.JsonResult)) {
                            response = _utilsServices
                                .GetResponse(ResponseType.MissingPolicies, "",
                                    JsonConvert.DeserializeObject(ResponseDictionary["PureJson"](requiredPolicies, _policyService)));
                        } else {
                            //This case should not ever happen
                            response = _utilsServices.GetResponse(ResponseType.None, "Unknown error.");
                        }


                        actionContext.Response = actionContext
                            .ControllerContext
                            .Request
                            .CreateResponse(HttpStatusCode.OK, response, "application/json");
                    }

                }
            }
        }
    }
}