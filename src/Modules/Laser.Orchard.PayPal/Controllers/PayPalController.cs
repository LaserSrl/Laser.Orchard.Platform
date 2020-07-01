using Laser.Orchard.PaymentGateway;
using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PayPal.Models;
using Laser.Orchard.PayPal.Services;
using Laser.Orchard.PayPal.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Themes;
using System.Web.Mvc;

namespace Laser.Orchard.PayPal.Controllers {
    public class PayPalController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly PayPalPosService _posService;
        //private readonly IBraintreeService _braintreeService;

        public PayPalController(
            IOrchardServices orchardServices, 
            IRepository<PaymentRecord> repository, 
            IPaymentEventHandler paymentEventHandler 
            /*,IBraintreeService braintreeService*/) {
            _orchardServices = orchardServices;
            _posService = new PayPalPosService(orchardServices, repository, paymentEventHandler);
            //_braintreeService = braintreeService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;

        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pid">Payment ID</param>
        /// <returns></returns>
        [Themed]
        public ActionResult Index(int pid = 0, string guid = "") {
            PaymentRecord payment;
            if (pid > 0) {
                payment = _posService.GetPaymentInfo(pid);
            }
            else {
                payment = _posService.GetPaymentInfo(guid);
            }
            pid = payment.Id;
            var settings = _orchardServices.WorkContext.CurrentSite.As<PayPalSiteSettingsPart>();
            if (settings.CurrencyCode != payment.Currency) {
                //throw new Exception(string.Format("Invalid currency code. Valid currency is {0}.", settings.CurrencyCode));
                string error = string.Format("Invalid currency code. Valid currency is {0}.", settings.CurrencyCode);
                _posService.EndPayment(payment.Id, false, error, error);
                return Redirect(_posService.GetPaymentInfoUrl(payment.Id));
            }
            PaymentVM model = new PaymentVM();
            model.Record = payment;
            model.TenantBaseUrl = Url.Action("Index").Replace("/Laser.Orchard.PayPal/PayPal", "");
            return View("Index", model);
        }
    }
}