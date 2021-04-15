using Laser.Orchard.PaymentGateway.Security;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Controllers {
    /// <summary>
    /// This class gets implemented in each payment gateway module. They will only need to implement GetSettingsPart. Everything else is handled here
    /// and used to display a page where the back-office user may configure the payment gateway. The corresponding view should be implemented in the
    /// specific module, of course.
    /// </summary>
    [Admin]
    public abstract class PosAdminBaseController : Controller {
        protected readonly IOrchardServices _orchardServices;
        protected readonly ISignals _signals;
        public Localizer T { get; set; }

        /// <summary>
        /// Get POS settings part to be used in Index view.
        /// </summary>
        /// <returns></returns>
        protected abstract ContentPart GetSettingsPart();

        protected virtual string CacheKey => "";

        public PosAdminBaseController(
            IOrchardServices orchardServices,
            ISignals signals) {

            _orchardServices = orchardServices;
            _signals = signals;
            T = NullLocalizer.Instance;
        }
        public ActionResult Index() {
            if (_orchardServices.Authorizer.Authorize(Permissions.ConfigurePayment) == false) {
                return new HttpUnauthorizedResult();
            }
            var settings = GetSettingsPart();
            return View(settings);
        }
        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost() {
            if (_orchardServices.Authorizer.Authorize(Permissions.ConfigurePayment) == false) {
                return new HttpUnauthorizedResult();
            }
            var settings = GetSettingsPart();
            if (TryUpdateModel((dynamic)settings)) {
                _orchardServices.Notifier.Information(T("Settings saved successfully."));
            }
            else {
                _orchardServices.Notifier.Error(T("Could not save settings."));
            }
            if (!string.IsNullOrWhiteSpace(CacheKey)) {
                _signals.Trigger(CacheKey);
            }
            return RedirectToAction("Index");
        }
    }
}