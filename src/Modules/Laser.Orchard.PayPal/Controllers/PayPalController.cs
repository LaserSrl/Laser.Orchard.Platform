using Laser.Orchard.PaymentGateway;
using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PayPal.Models;
using Laser.Orchard.PayPal.Services;
using Laser.Orchard.PayPal.ViewModels;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Themes;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace Laser.Orchard.PayPal.Controllers {
    public class PayPalController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly PayPalPosService _posService;
        private readonly IPayPalService _PayPalService;

        public PayPalController(
            IOrchardServices orchardServices, 
            IRepository<PaymentRecord> repository, 
            IPaymentEventHandler paymentEventHandler,
            IPayPalService PayPalService) {
            _orchardServices = orchardServices;
            _posService = new PayPalPosService(orchardServices, repository, paymentEventHandler);
            _PayPalService = PayPalService;

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

        [HttpPost]
        public ActionResult FinalizePayment(int pId) {
            CheckOrderResult result = new CheckOrderResult();
            var payment = _posService.GetPaymentInfo(pId);

            var orderId = _orchardServices.WorkContext.HttpContext.Request["OrderId"];
            if (string.IsNullOrWhiteSpace(orderId)) {
                result.Success = false;
                result.MessageError = "Order id cannot be empty";
            }
            else {
                result = _PayPalService.VerifyOrderIdPayPal(orderId);
            }

            _posService.EndPayment(pId, result.Success, result.MessageError, result.Info, orderId);
            if (!result.Success) {
                Logger.Error("Error on payment for order {0} Error: {1}", payment.ContentItemId, result.MessageError);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, result.MessageError);
            }
            
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public ActionResult ErrorPay(int pId) {
            // this controller manage only response negative.
            // is called into action onError manage from javascript PayPal
            CheckOrderResult result = new CheckOrderResult();
            result.Success = false;
            result.MessageError = string.Format("Something went wrong during the payment");

            var payment = _posService.GetPaymentInfo(pId);
            _posService.EndPayment(payment.Id, result.Success, result.MessageError, result.Info);

            Logger.Error("Error on payment for order {0} Error: {1}", payment.ContentItemId, result.MessageError);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}