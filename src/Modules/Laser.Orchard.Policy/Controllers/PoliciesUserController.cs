using Laser.Orchard.Policy.Services;
using Laser.Orchard.Policy.ViewModels;
using Orchard.Themes;
using Orchard.UI.Admin;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.Policy.Controllers {
    [Admin, Themed(false)]
    public class PoliciesUserController : Controller {

        private readonly IPolicyServices _policyServices;

        public PoliciesUserController(IPolicyServices policyServices) {
            _policyServices = policyServices;
        }

        public ActionResult ShowHistory(int userId) {
            List<PolicyHistoryViewModel> model = _policyServices.GetPolicyHistoryForUser(userId);
            return View(model);
        }
    }
}