using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Services;
using Laser.Orchard.PaymentGateway.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Logging;

namespace Laser.Orchard.PaymentGateway.Controllers {
    public class PaymentController : Controller {
        /// <summary>
        /// This class is a default implementation of the pos services, used as basically a placeholder when calling some methods, since abstract classes
        /// cannot be directly instantiated.
        /// </summary>
        private class PosServiceEmpty : PosServiceBase {
            public PosServiceEmpty(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler)
                : base(orchardServices, repository, paymentEventHandler) {
            }
            public override string GetPosName() {
                return "";
            }

            public override string GetPosActionUrl(int paymentId) {
                return "";
            }
            public override string GetPosActionUrl(string paymentGuid) {
                return "";
            }

            public override string GetSettingsControllerName() {
                return "";
            }

            public override string GetPosUrl(int paymentId) {
                return "";
            }

            public override Type GetPosActionControllerType() {
                return typeof(object);
            }

            public override string GetPosActionName() {
                return "";
            }
        }
        private readonly IRepository<PaymentRecord> _repository;
        private readonly IOrchardServices _orchardServices;
        private readonly IEnumerable<IPosService> _posServices;
        private readonly PosServiceEmpty _posServiceEmpty;
        private readonly IPaymentService _paymentService;
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public PaymentController(
            IRepository<PaymentRecord> repository, 
            IOrchardServices orchardServices, 
            IEnumerable<IPosService> posServices, 
            IPaymentService paymentService) {

            _repository = repository;
            _orchardServices = orchardServices;
            _paymentService = paymentService;
            _posServices = posServices;
            _posServiceEmpty = new PosServiceEmpty(orchardServices, repository, null);
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }
        /// <summary>
        /// This controller starts the web flow for payments, and creates the associated record
        /// </summary>
        /// <param name="reason">String describing the reason for the payment</param>
        /// <param name="amount">Amount of payment</param>
        /// <param name="currency">Currency used</param>
        /// <param name="itemId">Optional, the Id of a ContentItem associated with the payment</param>
        /// <param name="newPaymentGuid">Guid to be associated with this payment. No previous payment should have this value.</param>
        /// <returns>A page proposing the paymet options</returns>
        [Themed]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Pay(string nonce, string newPaymentGuid = null) {
            var record = _paymentService.DecryptPaymentNonce(nonce);
            if(record == null) {
                Logger.Error("Error decrypting payment nonce.");
                return new HttpUnauthorizedResult();
            }
            ContentItem item = null;
            if (record.ContentItemId > 0) {
                item = _orchardServices.ContentManager.Get(record.ContentItemId, VersionOptions.Latest);
            }
            PaymentVM model = new PaymentVM {
                Record = record,
                PosList = _posServices.ToList(),
                ContentItem = item
            };
            try {
                model.Record = _posServiceEmpty.StartPayment(model.Record, newPaymentGuid);
            }
            catch(Exception ex) {
                Logger.Information(ex, "Error starting payment.");
                return new HttpUnauthorizedResult();
            }
            return View("Pay", model);
        }
        /// <summary>
        /// This controller shows information about a specific payment
        /// </summary>
        /// <param name="paymentId">An Id corresponding to the record that contains the payment information</param>
        /// <param name="guid">The Guid corresponding to the payment. The check on this value is performed only for anonymous users.</param>
        /// <returns>A page reporting the information for the payment</returns>
        [Themed]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Info(int paymentId, string guid = "") {
            int currentUserId = -1; // utente inesistente
            var user = _orchardServices.WorkContext.CurrentUser;
            if (user != null) {
                currentUserId = user.Id;
            }
            var payment = _repository.Get(paymentId);
            var unauthorized = false;
            if (currentUserId == -1) {
                // user anonimo
                if (guid != payment.Guid) {
                    unauthorized = true;
                }
            }
            else {
                // user autenticato
                if ((_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner) == false)
                    && (payment.UserId != currentUserId)) {
                    unauthorized = true;
                }
            }
            if (unauthorized) {
                return new HttpUnauthorizedResult();
            }
            var model = new PaymentVM();
            model.Record = payment;
            model.PaymentNonce = _paymentService.CreatePaymentNonce(payment);
            return View("Info", model);
        }

        /// <summary>
        /// This method tells whether a given payment has been terminated. The payment is identified by either its Id or its Guid.
        /// If no payment is identified by the parameters, we return false, as if the transaction had been completed.
        /// </summary>
        /// <param name="paymentId">The Id of the payment we are querying about</param>
        /// <param name="guid">The guid of the payment we are querying about</param>
        /// <returns>A Json telling whether the transaction still has to be completed (true) or has finished (false).</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PaymentIsIncomplete(int paymentId = 0, string guid = "") {
            PaymentRecord payment = null;
            try {
                if (paymentId > 0) {
                    //this version of GetPaymentInfo throws an exception if the id is not valid
                    payment = _posServiceEmpty.GetPaymentInfo(paymentId);
                } else {
                    //this version of GetPaymentInfo throws an exception if the guid is not valid
                    payment = _posServiceEmpty.GetPaymentInfo(guid);
                }
            } catch (Exception) {
                payment = null;
            }
            if (payment == null) {
                return Json(new { Success = false } );
            }
            return Json(new { Success = !payment.PaymentTransactionComplete });
        }
    }
}