using Laser.Orchard.PaymentGestPay.Models;
using Laser.Orchard.PaymentGestPay.Services;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGestPay.Controllers {
    public class TransactionsController : Controller {

        private IGestPayTransactionServices _gestPayTransactionServices;
        private dynamic Shape { get; set; }
        public Localizer T { get; set; }

        private IOrchardServices _orchardServices;

        public TransactionsController(IShapeFactory shapeFactory, IGestPayTransactionServices gestPayTransactionServices, IOrchardServices orchardServices) {
            _gestPayTransactionServices = gestPayTransactionServices;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
            _orchardServices = orchardServices;
        }

        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult RedirectToGestPayPage(int Id = 0, string guid = "") {
            try {
                if (Id > 0) {
                    string posUrl = _gestPayTransactionServices.GetPosUrl(Id);
                    return Redirect(posUrl);
                } else {
                    string posUrl = _gestPayTransactionServices.GetPosUrl(guid);
                    return Redirect(posUrl);
                }
            } catch (Exception) {
                return new HttpUnauthorizedResult();
            }
        }

        /// <summary>
        /// GestPay calls this controller while proceeding with the transaction
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public ActionResult GestPayS2SEndpoint(string a, string b) {
            TransactionOutcome outcome = _gestPayTransactionServices.ReceiveS2STransaction(a, b);
            //the mesages given in the view are for debug purposes, because GestPay does not care
            string shapeString = "S2S";

            if (!string.IsNullOrWhiteSpace(outcome.TransactionResult)) {
                if (outcome.TransactionResult == "KO") {
                    shapeString += T(" Transaction failed with error: {0}", outcome.ErrorDescription).Text;
                } else if (outcome.TransactionResult == "OK") {
                    shapeString += T("Transaction successful.").Text;
                } else {
                    shapeString += T(" Transactions still in progress.").Text;
                }
            }

            Shape.Outcome = shapeString;
            return View(Shape); ;
        }

        /// <summary>
        /// GestPay redirects the buyer to these actions when it's done
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public ActionResult GestPaySuccess(string a, string b) {
            return RedirectToAction("GestPayOutcome", new { a = a, b = b });
        }
        /// <summary>
        /// GestPay redirects the buyer to these actions when it's done
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public ActionResult GestPayFailure(string a, string b) {
            return RedirectToAction("GestPayOutcome", new { a = a, b = b });
        }
        public ActionResult GestPayOutcome(string a, string b) {
            string redUrl = _gestPayTransactionServices.InterpretTransactionResult(a, b);

            return Redirect(redUrl);
        }
    }


}