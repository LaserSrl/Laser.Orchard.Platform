using Laser.Orchard.StartupConfig.Services;
using System;
using System.Web.Mvc;
//using Laser.Orchard.Mobile.Services;

namespace Laser.Orchard.Mobile.Controllers {
    public class SignalController : Controller {
        private readonly IActivityServices _activityServices;
        public SignalController(IActivityServices activityServices) {
            _activityServices = activityServices;
        }
        public ActionResult Trigger(string signalName, int contentId, string returnUrl = "") {
            try {
                _activityServices.TriggerSignal(signalName, contentId);
                if (String.IsNullOrWhiteSpace(returnUrl)) {
                    return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
                } else {
                    return Redirect(returnUrl);
                }
            } catch (Exception ex) {
                if (String.IsNullOrWhiteSpace(returnUrl)) {
                    return Json(new { Success = false, ErrorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
                } else {
                    return Redirect(returnUrl);
                }
            }
        }

    }
}
