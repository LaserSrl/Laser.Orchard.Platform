using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.ViewModels;
using OrchardNS = Orchard;
using Orchard.ContentManagement;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Themes;
using Orchard.Mvc.Extensions;
using Laser.Orchard.Policy.Services;
using System.Text;

namespace Laser.Orchard.Policy.Controllers {
    [Themed]
    [RoutePrefix("UserPolicies")]
    public class PoliciesController : Controller {
        private readonly IPolicyServices _policyServices;


        public PoliciesController(IPolicyServices policyServices) {
            _policyServices = policyServices;

        }
        //
        // GET: /Policies/
        public ActionResult Index(string lang = null, string policies = null, bool editMode = false) {
            PoliciesForUserViewModel model = _policyServices.GetPoliciesForCurrentUser(false, lang);

            if (policies != null)
            {
                string decodedPolicies = Encoding.UTF8.GetString(Convert.FromBase64String(policies));
                if (!String.IsNullOrWhiteSpace(decodedPolicies))
                {
                    var policyIds = decodedPolicies.Replace("{", "").Replace("}", "").Split(',');
                    model.Policies = model.Policies.Where(x => policyIds.Contains(x.PolicyTextId.ToString())).ToList();
                }
            }

            model.EditMode = editMode;

            return View(model);
        }

        [HttpPost, ActionName("SavePolicies")]
        [OrchardNS.Mvc.FormValueRequired("submit.Save")]
        public ActionResult Index(string lang = null, string returnUrl = null, string policies = null, bool editMode = false) {
            PoliciesForUserViewModel model = _policyServices.GetPoliciesForCurrentUser(true, lang);

            if (TryUpdateModel(model, String.Empty)) {
                _policyServices.PolicyForUserMassiveUpdate(model.Policies);
                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index", new { EditMode = editMode }));
            } else {
                return View(model);
            }
        }
    }
}