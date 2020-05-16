using Laser.Orchard.StartupConfig.Services;
using System;
using System.Web.Mvc;

namespace Laser.Orchard.WebServices.Controllers {
    public class SignalController : Controller {
        private readonly IActivityServices _activityServices;
        public SignalController(IActivityServices activityServices) {
            _activityServices = activityServices;
        }
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Trigger(string signalName, int contentId, string returnUrl = "") {
            try {
                var response = _activityServices.TriggerSignal(signalName, contentId);
                if (String.IsNullOrWhiteSpace(returnUrl)) {
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                else {
                    return Redirect(returnUrl);
                }
            }
            catch (Exception ex) {
                if (String.IsNullOrWhiteSpace(returnUrl)) {
                    return Json(new { Success = false, ErrorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
                }
                else {
                    return Redirect(returnUrl);
                }
            }
        }
    }
}
