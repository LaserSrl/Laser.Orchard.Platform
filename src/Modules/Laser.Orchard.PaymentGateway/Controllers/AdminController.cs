using Laser.Orchard.PaymentGateway.Security;
using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.UI.Notify;
using System.Web.Mvc;
using Orchard.Localization;

namespace Laser.Orchard.PaymentGateway.Controllers {
    public class AdminController : Controller {
        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }

        public AdminController(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }
        //this loads a generic tab in the payment gateway settings section of the admin menu
        public ActionResult Index() {
            if (_orchardServices.Authorizer.Authorize(Permissions.ConfigurePayment) == false) {
                return new HttpUnauthorizedResult();
            }
            var settings = _orchardServices.WorkContext.CurrentSite.As<PaymentGatewaySiteSettingsPart>();
            return View("Index", settings);
        }
        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost() {
            if (_orchardServices.Authorizer.Authorize(Permissions.ConfigurePayment) == false) {
                return new HttpUnauthorizedResult();
            }
            var settings = _orchardServices.WorkContext.CurrentSite.As<PaymentGatewaySiteSettingsPart>();
            if (TryUpdateModel((dynamic)settings)) {
                _orchardServices.Notifier.Information(T("Settings saved successfully."));
            } else {
                _orchardServices.Notifier.Error(T("Could not save settings."));
            }
            return RedirectToAction("Index");
        }
    }
}