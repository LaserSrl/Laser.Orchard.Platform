using Laser.Orchard.Commons.Attributes;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.UsersExtensions.Models;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Laser.Orchard.UsersExtensions.Controllers {
    public class PoliciesController : Controller {

        private readonly IOrchardServices _orchardServices;
        private readonly IPolicyServices _policyServices;
        private readonly IWorkContextAccessor _workContext;

        public PoliciesController(IOrchardServices orchardServices, IPolicyServices policyServices, IWorkContextAccessor workContext) {
            _orchardServices = orchardServices;
            _policyServices = policyServices;
            _workContext = workContext;
        }

        [HttpGet]
        [AdminService]
        [OutputCache(NoStore = true, Duration = 0)]
        public JsonResult GetPolicyList(string lang = null, PoliciesRequestType type = PoliciesRequestType.All) {
            string outputFormat = _workContext.GetContext().HttpContext.Request.Headers["OutputFormat"];
            IEnumerable<PolicyTextInfoPart> policies = _policyServices.GetPolicies(lang);

            var orchardUsersSettings = _orchardServices.WorkContext.CurrentSite.As<UserRegistrationSettingsPart>();
            var registrationPoliciesIds = new Int32[0];
            if (orchardUsersSettings.PolicyTextReferences.Contains("{All}"))
                registrationPoliciesIds = policies.Select(x => x.Id).ToArray();
            else
                registrationPoliciesIds = orchardUsersSettings.PolicyTextReferences.Select(x => Convert.ToInt32(x.Replace("{", "").Replace("}", ""))).ToArray();

            if (type == PoliciesRequestType.ForRegistration)
                policies = policies.Where(x => registrationPoliciesIds.Contains(x.Id));
            else if (type == PoliciesRequestType.NotForRegistration)
                policies = policies.Where(x => !registrationPoliciesIds.Contains(x.Id));

            var jsonResult = "";
            if (String.Equals(outputFormat, "LMNV", StringComparison.OrdinalIgnoreCase))
                jsonResult = _policyServices.PoliciesLMNVSerialization(policies);
            else
                jsonResult = _policyServices.PoliciesPureJsonSerialization(policies);

            var serializer = new JavaScriptSerializer();
            return Json(serializer.DeserializeObject(jsonResult), JsonRequestBehavior.AllowGet);
        }
    }
}