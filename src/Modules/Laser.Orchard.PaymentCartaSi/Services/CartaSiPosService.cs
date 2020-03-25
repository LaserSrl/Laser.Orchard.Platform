using Laser.Orchard.PaymentCartaSi.Controllers;
using Laser.Orchard.PaymentCartaSi.Extensions;
using Laser.Orchard.PaymentCartaSi.Models;
using Laser.Orchard.PaymentGateway;
using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentCartaSi.Services {
    public class CartaSiPosService : PosServiceBase, ICartaSiTransactionService {

        public ILogger Logger { get; set; }

        public CartaSiPosService(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler) :
            base(orchardServices, repository, paymentEventHandler) {

            Logger = NullLogger.Instance;
        }

        /// <summary>
        /// Get the string that we use to identify the payment method
        /// </summary>
        /// <returns></returns>
        public override string GetPosName() {
            return Constants.PosName;
        }
        public override string GetSettingsControllerName() {
            return "Admin";
        }
        /// <summary>
        /// This gets called by the "general" payment services to get the url of an action that will start the operations on the virtual POS
        /// </summary>
        /// <param name="paymentId">The id corresponding to a <type>PaymentRecord</type> for the transaction we want to start.</param>
        /// <returns>The url corresponding to an action that will start the CartaSì transaction </returns>
        public override string GetPosActionUrl(int paymentId) {
            //create the url for the controller action that takes care of the redirect, passing the id as parameter
            //Controller: Transactions
            //Action; RedirectToCartaSìPage
            //Area: Laser.Orchard.PaymentCartaSi
            var hp = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            var ub = new UriBuilder(_orchardServices.WorkContext.HttpContext.Request.Url.AbsoluteUri) {
                Path = hp.Action("RedirectToCartaSìPage", "Transactions", new { Area = Constants.LocalArea, Id = paymentId }),
                Query = ""
            };
            return ub.Uri.ToString();
        }
        /// <summary>
        /// This gets called by the "general" payment services to get the url of an action that will start the operations on the virtual POS
        /// </summary>
        /// <param name="paymentId">The Guid corresponding to a <type>PaymentRecord</type> for the transaction we want to start.</param>
        /// <returns>The url corresponding to an action that will start the CartaSì transaction </returns>
        public override string GetPosActionUrl(string paymentGuid) {
            //create the url for the controller action that takes care of the redirect, passing the id as parameter
            //Controller: Transactions
            //Action; RedirectToCartaSìPage
            //Area: Laser.Orchard.PaymentCartaSi
            var hp = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            var ub = new UriBuilder(_orchardServices.WorkContext.HttpContext.Request.Url.AbsoluteUri) {
                Path = hp.Action("RedirectToCartaSìPage", "Transactions", new { Area = Constants.LocalArea }),
                Query = "guid=" + paymentGuid
            };
            return ub.Uri.ToString();
        }

        public override Type GetPosActionControllerType() {
            return typeof(TransactionsController);
        }
        public override string GetPosActionName() {
            return "RedirectToCartaSìPage";
        }

        /// <summary>
        /// This gets the url of the virtual pos.
        /// </summary>
        /// <param name="paymentId">The id of a payment record for the current transaction</param>
        /// <returns>the url of the virtual pos</returns>
        public override string GetPosUrl(int paymentId) {
            return StartCartaSiTransactionURL(paymentId);
        }
        /// <summary>
        /// returns a list of currencies that we are allowed to use with cartasì
        /// </summary>
        /// <returns></returns>
        public override List<string> GetAllValidCurrencies() {
            //Carta sì accepts only payments in Euro
            return new string[] { "EUR" }.ToList();
        }

        /// <summary>
        /// Compute the full url for an Action in a Controller in the current site.
        /// </summary>
        /// <param name="aName">The name of the action.</param>
        /// <param name="cName">The name of the controller. Defaults at "Transactions".</param>
        /// <param name="areaName">The area of the controller. Defaults at the local area for this module.</param>
        /// <returns>The full Url of the action.</returns>
        private string ActionUrl(string aName, string cName = "Transactions", string areaName = Constants.LocalArea) {
            string sName = _orchardServices.WorkContext.CurrentSite.SiteName;
            string bUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
            var hp = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            string aPath = hp.Action(aName, cName, new { Area = areaName });
            int cut = aPath.ToUpperInvariant().IndexOf(sName.ToUpperInvariant()) - 1;
            return bUrl + aPath.Substring(cut);
        }
#if DEBUG
        private string RemoteActionUrl(string aName, string cName = "Transactions", string areaName = Constants.LocalArea) {
            string sName = _orchardServices.WorkContext.CurrentSite.SiteName;
            string bUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
            var hp = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            string aPath = hp.Action(aName, cName, new { Area = areaName });
            string outSite = "http://piovanellim.laser-group.com";
            return outSite + aPath;
        }
#endif
        /// <summary>
        /// Computes the url of CartaSì's web service to which the buyer has to be redirected.
        /// </summary>
        /// <param name="paymentId">The id of the PaymentRecord for the transaction we are trying to complete.</param>
        /// <returns>The url where we should redirct the buyer.</returns>
        public string StartCartaSiTransactionURL(int paymentId) {
            var settings = _orchardServices.WorkContext.CurrentSite.As<PaymentCartaSiSettingsPart>();

            string pURL = settings.UseTestEnvironment ? EndPoints.TestPaymentURL : EndPoints.PaymentURL;
            var pRecord = GetPaymentInfo(paymentId);
            if (pRecord.PaymentTransactionComplete) {
                //this avoids repeat payments when the user is dumb and goes back in the browser to try and pay again
                return GetPaymentInfoUrl(paymentId);
            }
            var user = _orchardServices.WorkContext.CurrentUser;
            if (pRecord.UserId > 0 && pRecord.UserId != user.Id) {
                //not the same user who started the payment
                throw new Exception();
            }
            StartPaymentMessage spMsg = new StartPaymentMessage(settings.CartaSiShopAlias, settings.CartaSiSecretKey, pRecord);
            spMsg.url = ActionUrl("CartaSiOutcome");
            spMsg.url_back = ActionUrl("CartaSiUndo");
            spMsg.urlpost = ActionUrl("CartaSiS2S");
#if DEBUG
            spMsg.urlpost=RemoteActionUrl("CartaSiS2S");
#endif
            spMsg.mac = spMsg.TransactionStartMAC;


            try {
                Validator.ValidateObject(spMsg, new ValidationContext(spMsg), true);
            } catch (Exception ex) {
                //Log the error
                Logger.Error(T("Transaction information not valid: {0}", ex.Message).Text);
                //update the PaymentRecord for this transaction
                EndPayment(paymentId, false, null, T("Transaction information not valid: {0}", ex.Message).Text);
                //return the URL of a suitable error page (call this.GetPaymentInfoUrl after inserting the error in the PaymentRecord)
                return GetPaymentInfoUrl(paymentId);
            }

            //from the parameters, make the query string for the payment request
            string qString = "";
            try {
                qString = spMsg.MakeQueryString();
                if (string.IsNullOrWhiteSpace(qString)) {
                    throw new Exception(T("Errors while creating the query string. The query string cannot be empty.").Text);
                }
            } catch (Exception ex) {
                //Log the error
                Logger.Error(ex.Message);
                //update the PaymentRecord for this transaction
                EndPayment(paymentId, false, null, ex.Message);
                //return the URL of a suitable error page (call this.GetPaymentInfoUrl after inserting the error in the PaymentRecord)
                return GetPaymentInfoUrl(paymentId);
            }

            pURL = string.Format("{0}?{1}", pURL, qString);
            return pURL; // return null;
        }
        /// <summary>
        /// Handles errors happening on cartasì's side, including the buyers canceling the transaction.
        /// </summary>
        /// <param name="importo"></param>
        /// <param name="divisa"></param>
        /// <param name="codTrans"></param>
        /// <param name="esito"></param>
        /// <returns>An url where the buyer should be redirected.</returns>
        public string ReceiveUndo(string importo, string divisa, string codTrans, string esito) {
            int id;
            if (int.TryParse(codTrans.Replace("LASER", ""), out id)) {
                LocalizedString error;
                if (esito.ToUpperInvariant() == "ANNULLO") {
                    error = T("Transaction canceled.");
                } else if (esito.ToUpperInvariant() == "ERRORE") {
                    error = T("Formal error in the call.");
                } else {
                    error = T("Unknown error.");
                }
                EndPayment(id, false, error.Text, error.Text);
                return GetPaymentInfoUrl(id);
            } else {
                //Log the error
                LocalizedString error = T("Received wrong information while coming back from payment: wrong Id format.");
                Logger.Error(error.Text);
                throw new Exception(error.Text);
            }
        }
        /// <summary>
        /// handles the server-to-server transaction happening when cartasì wants to report the end of a transaction
        /// </summary>
        /// <param name="qs"></param>
        /// <returns></returns>
        public string HandleS2STransaction(NameValueCollection qs) {
            var settings = _orchardServices.WorkContext.CurrentSite.As<PaymentCartaSiSettingsPart>();
            //this is the method where the transaction information is trustworthy
            StringBuilder sr = new StringBuilder();
            int paymentId = 0; //assign here because compiler does not understand that we won't use this without assigning it first
            bool validMessage = !string.IsNullOrWhiteSpace(qs["codTrans"]) && int.TryParse(qs["codTrans"].Replace("LASER", ""), out paymentId); //has an id
            validMessage = validMessage && !string.IsNullOrWhiteSpace(qs["esito"]); //has a result
            validMessage = validMessage && !string.IsNullOrWhiteSpace(qs["alias"]) && qs["alias"] == settings.CartaSiShopAlias; //has right shop alias
            //Logger.Error("HandleS2STransaction: " + paymentId.ToString());
            if (validMessage) {
                PaymentOutcomeMessage pom = new PaymentOutcomeMessage(qs);
                pom.secret = settings.CartaSiSecretKey;
                //sr.Clear();
                sr.AppendLine("HandleS2STransaction: MESSAGE VALID");
                sr.AppendLine(pom.ToString());
                //Logger.Error(sr.ToString());
                try {
                    Validator.ValidateObject(pom, new ValidationContext(pom), true);
                } catch (Exception ex) {
                    //Logger.Error(ex.Message);
                    //throw ex;
                    LocalizedString error = T(@"Transaction information not valid for transaction {0}: {1}", paymentId.ToString(), ex.Message);
                    //Log the error
                    sr.AppendLine(string.Format("ERROR: {0}", error.Text));
                    //Logger.Error(error.Text);
                    //throw new Exception(error.Text);
                    //We do not update the PaymentRecord here, because we have been unable to verify the hash that we received

                }
                //Logger.Error("HandleS2STransaction: VALIDATION PASSED");
                //verify the hash
                if (pom.PaymentOutcomeMAC == qs["mac"]) {
                    //transaction valid
                    //update the PaymentRecord for this transaction
                    //TODO: add to info the decoding of the pom.codiceEsito based off the codetables
                    string info = CodeTables.ErrorCodes[int.Parse(pom.codiceEsito)];
                    EndPayment(paymentId, pom.esito == "OK", pom.codiceEsito, pom.messaggio + (string.IsNullOrWhiteSpace(info) ? "" : (" " + info)));
                    //Logger.Error(string.Format(@"Payment {0} S2S outcome {1}", paymentId.ToString(), pom.esito));
                    //return the URL of a suitable error page (call this.GetPaymentInfoUrl after inserting the error in the PaymentRecord)
                    return pom.esito;
                } else {
                    LocalizedString error = T("HandleS2STransaction: MAC NOT VALID:\nComputed: {0}\nReceived: {1}", pom.PaymentOutcomeMAC, qs["mac"]);
                    sr.AppendLine(string.Format("ERROR: {0}", error.Text));
                }

            }
            Logger.Error(sr.ToString());
            throw new Exception(string.Format("Transaction message not valid: codTrans: {0}, esito: {1}, alias: {2}", qs["codTrans"] ?? "null", qs["esito"] ?? "null", qs["alias"] ?? "null"));
        }

        /// <summary>
        /// Gets the inforation about the transaction result back from CartaSì and returns an URL showing the transaction's result.
        /// Depending on the way the transaction was set, this may not actually be an url.
        /// </summary>
        /// <param name="qs">The query string received in the attempt by CartaSì to redirect the browser.</param>
        /// <returns>The Url for the transaction results.</returns>
        public string HandleOutcomeTransaction(NameValueCollection qs) {
            //transaction information here may not be trustworthy
            int paymentId;
            if (!string.IsNullOrWhiteSpace(qs["codTrans"]) && int.TryParse(qs["codTrans"].Replace("LASER", ""), out paymentId)) {
                PaymentOutcomeMessage pom = new PaymentOutcomeMessage(qs);
                PaymentRecord pRecord = GetPaymentInfo(paymentId);
                if (pRecord != null) {
                    return GetPaymentInfoUrl(paymentId);
                }
            }

            LocalizedString error = T("Impossible to identify transaction. There was a communication error between CartaSì and our servers");
            Logger.Error(error.Text);
            throw new Exception(error.Text);
        }
    }
}