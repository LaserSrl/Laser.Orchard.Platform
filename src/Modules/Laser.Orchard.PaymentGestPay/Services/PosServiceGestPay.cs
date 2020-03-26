using Laser.Orchard.PaymentGateway;
using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Services;
using Laser.Orchard.PaymentGestPay.Controllers;
using Laser.Orchard.PaymentGestPay.CryptDecryptProd;
using Laser.Orchard.PaymentGestPay.CryptDecryptTest;
using Laser.Orchard.PaymentGestPay.Extensions;
using Laser.Orchard.PaymentGestPay.Models;
using Laser.Orchard.PaymentGestPay.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;

namespace Laser.Orchard.PaymentGestPay.Services {
    public class PosServiceGestPay : PosServiceBase, IGestPayAdminServices, IGestPayTransactionServices {


        public ILogger Logger { get; set; }

        public PosServiceGestPay(
            IOrchardServices orchardServices,
            IRepository<PaymentRecord> repository,
            IPaymentEventHandler paymentEventHandler) :
            base(orchardServices, repository, paymentEventHandler) {

            Logger = NullLogger.Instance;

        }

        #region Implementation of abstract class
        /// <summary>
        /// Gets the string used to identify this payment method
        /// </summary>
        /// <returns>GestPay</returns>
        public override string GetPosName() {
            return "GestPay";
        }
        /// <summary>
        /// This gets called by the "general" payment services to get a link to the action that will start the payment
        /// </summary>
        /// <param name="paymentId">The id corresponding to a <type>PaymentRecord</type> for the transaction we want to start.</param>
        /// <returns>The url corresponding to an action that will start the GestPay transaction </returns>
        public override string GetPosActionUrl(int paymentId) {
            //create the url for the controller action that takes care of the redirect, passing the id as parameter
            //Controller: Transactions
            //Action: RedirectToGestPayPage
            //Area: Laser.Orchard.PaymentGestPay
            var hp = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            var ub = new UriBuilder(_orchardServices.WorkContext.HttpContext.Request.Url.AbsoluteUri) {
                Path = hp.Action("RedirectToGestPayPage", "Transactions", new { Area = Constants.LocalArea, Id = paymentId }),
                Query = ""//"Id=" + paymentId.ToString()
            };
            //var ub = new UrlHelper().Action("RedirectToGestPayPage", "Transactions", new { Id = paymentId });
            return ub.Uri.ToString();
        }
        /// <summary>
        /// This gets called by the "general" payment services to get a link to the action that will start the payment
        /// </summary>
        /// <param name="paymentId">The Guid corresponding to a <type>PaymentRecord</type> for the transaction we want to start.</param>
        /// <returns>The url corresponding to an action that will start the GestPay transaction </returns>
        public override string GetPosActionUrl(string paymentGuid) {
            //create the url for the controller action that takes care of the redirect, passing the id as parameter
            //Controller: Transactions
            //Action: RedirectToGestPayPage
            //Area: Laser.Orchard.PaymentGestPay
            var hp = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            var ub = new UriBuilder(_orchardServices.WorkContext.HttpContext.Request.Url.AbsoluteUri) {
                Path = hp.Action("RedirectToGestPayPage", "Transactions", new { Area = Constants.LocalArea }),
                Query = "guid=" + paymentGuid
            };
            //var ub = new UrlHelper().Action("RedirectToGestPayPage", "Transactions", new { Id = paymentId });
            return ub.Uri.ToString();
        }

        public override Type GetPosActionControllerType() {
            return typeof(TransactionsController);
        }
        public override string GetPosActionName() {
            return "RedirectToGestPayPage";
        }

        /// <summary>
        /// Gets the name of the controller where the Action for the editor of the POS settings is.
        /// </summary>
        /// <returns>The name of that controller.</returns>
        public override string GetSettingsControllerName() {
            return "Admin";
        }
        /// <summary>
        /// Gets the URL of the virtual POS where we wish to redirect the user for payment
        /// </summary>
        /// <param name="paymentId">The id of a payment record</param>
        /// <returns>The url to redirect the user to, as a string.</returns>
        public override string GetPosUrl(int paymentId) {
            return StartGestPayTransactionURL(paymentId);
        }
        /// <summary>
        /// Gets a list of valid currencies for GestPay
        /// </summary>
        /// <returns>A list of the alpha 3 iso representation of the currencies that are allowed for GestPay</returns>
        public override List<string> GetAllValidCurrencies() {
            return CodeTables.CurrencyCodes.Select(cc => cc.isoCode).ToList();
        }
        #endregion

        #region Admin Methods
        public GestPaySettingsViewModel GetSettingsVM() {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<PaymentGestPaySettingsPart>();

            return new GestPaySettingsViewModel(settings);
        }

        public void UpdateSettings(GestPaySettingsViewModel vm) {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<PaymentGestPaySettingsPart>();

            settings.GestPayShopLogin = vm.GestPayShopLogin;
            settings.UseTestEnvironment = vm.UseTestEnvironment;
        }
        #endregion

        /// <summary>
        /// This gets called by the Action actually starting the transaction.
        /// </summary>
        /// <param name="paymentId">The id corresponding to a <type>PaymentRecord</type> for the transaction we want to start.</param>
        /// <returns>The url of a page to which we redirect the client's browser to complete the payment.</returns>
        private string StartGestPayTransactionURL(int paymentId) {

            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<PaymentGestPaySettingsPart>();

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
            var gpt = new GestPayTransaction(pRecord);
            //parameter validation
            if (gpt == null) {
                //Log the error
                Logger.Error(T("Transaction object cannot be null.").Text);
                //update the PaymentRecord for this transaction
                EndPayment(paymentId, false, null, T("Failed to create a transaction object based on the PaymentRecord").Text);
                //return the url of a suitable error page (call this.GetPaymentInfoUrl after inserting the error in the PaymentRecord)
                return GetPaymentInfoUrl(paymentId);
            }

            try {
                Validator.ValidateObject(gpt, new ValidationContext(gpt), true);
            } catch (Exception ex) {
                //Log the error
                Logger.Error(T("Transaction information not valid: {0}", ex.Message).Text);
                //update the PaymentRecord for this transaction
                EndPayment(paymentId, false, null, T("Transaction information not valid: {0}", ex.Message).Text);
                //return the URL of a suitable error page (call this.GetPaymentInfoUrl after inserting the error in the PaymentRecord)
                return GetPaymentInfoUrl(paymentId);
            }

            //get the encrypted parameter string
            EncryptDecryptTransactionResult res = null;
            string urlFormat = "";
            XmlNode encryptXML = null;
            try {
                if (settings.UseTestEnvironment) {
                    string endpoint = string.Format(Endpoints.TestWSEntry, Endpoints.CryptDecryptEndPoint);
                    endpoint = endpoint.Substring(0, endpoint.Length - 4);
                    //WSHttpBinding binding = new WSHttpBinding();
                    //binding.Security.Mode = SecurityMode.Transport;
                    //binding.MessageEncoding = WSMessageEncoding.Text;
                    BasicHttpBinding binding = new BasicHttpBinding();
                    endpoint = Regex.Replace(endpoint, "(https)", "http"); //https gives errors
                    EndpointAddress address = new EndpointAddress(endpoint);

                    using (var client = new CryptDecryptTest.WSCryptDecryptSoapClient(binding, address)) {

                        encryptXML = client.Encrypt(
                            shopLogin: settings.GestPayShopLogin,
                            uicCode: gpt.uicCode,
                            amount: gpt.amount,
                            shopTransactionId: gpt.shopTransactionID,
                            cardNumber: gpt.cardNumber,
                            expiryMonth: gpt.expiryMonth,
                            expiryYear: gpt.expiryYear,
                            buyerName: gpt.buyerName,
                            buyerEmail: gpt.buyerEmail,
                            languageId: gpt.languageId,
                            cvv: gpt.cvv,
                            customInfo: gpt.customInfo,
                            requestToken: gpt.requestToken,
                            ppSellerProtection: gpt.ppSellerProtection,
                            shippingDetails: gpt.shippingDetails.TestVersion(),
                            paymentTypes: gpt.paymentTypes.ToArray(),
                            paymentTypeDetail: gpt.paymentTypeDetail.TestVersion(),
                            redFraudPrevention: gpt.redFraudPrevention,
                            Red_CustomerInfo: gpt.Red_CustomerInfo.TestVersion(),
                            Red_ShippingInfo: gpt.Red_ShippingInfo.TestVersion(),
                            Red_BillingInfo: gpt.Red_BillingInfo.TestVersion(),
                            Red_CustomerData: gpt.Red_CustomerData.TestVersion(),
                            Red_CustomInfo: gpt.Red_CustomInfo.ToArray(),
                            Red_Items: gpt.Red_Items.TestVersion(),
                            Consel_MerchantPro: gpt.Consel_MerchantPro,
                            Consel_CustomerInfo: gpt.Consel_CustomerInfo.TestVersion(),
                            payPalBillingAgreementDescription: gpt.payPalBillingAgreementDescription,
                            OrderDetails: gpt.OrderDetails.TestVersion()
                        );
                        urlFormat = string.Format(Endpoints.TestPayEntry, Endpoints.PaymentPage);
                    }
                } else {
                    string endpoint = string.Format(Endpoints.ProdWSEntry, Endpoints.CryptDecryptEndPoint);
                    endpoint = endpoint.Substring(0, endpoint.Length - 4);
                    BasicHttpBinding binding = new BasicHttpBinding();
                    endpoint = Regex.Replace(endpoint, "(https)", "http"); //https gives errors
                    EndpointAddress address = new EndpointAddress(endpoint);

                    using (var client = new CryptDecryptProd.WSCryptDecryptSoapClient(binding, address)) {
                        encryptXML = client.Encrypt(
                            shopLogin: settings.GestPayShopLogin,
                            uicCode: gpt.uicCode,
                            amount: gpt.amount,
                            shopTransactionId: gpt.shopTransactionID,
                            cardNumber: gpt.cardNumber,
                            expiryMonth: gpt.expiryMonth,
                            expiryYear: gpt.expiryYear,
                            buyerName: gpt.buyerName,
                            buyerEmail: gpt.buyerEmail,
                            languageId: gpt.languageId,
                            cvv: gpt.cvv,
                            customInfo: gpt.customInfo,
                            requestToken: gpt.requestToken,
                            ppSellerProtection: gpt.ppSellerProtection,
                            shippingDetails: gpt.shippingDetails.ProdVersion(),
                            paymentTypes: gpt.paymentTypes.ToArray(),
                            paymentTypeDetail: gpt.paymentTypeDetail.ProdVersion(),
                            redFraudPrevention: gpt.redFraudPrevention,
                            Red_CustomerInfo: gpt.Red_CustomerInfo.ProdVersion(),
                            Red_ShippingInfo: gpt.Red_ShippingInfo.ProdVersion(),
                            Red_BillingInfo: gpt.Red_BillingInfo.ProdVersion(),
                            Red_CustomerData: gpt.Red_CustomerData.ProdVersion(),
                            Red_CustomInfo: gpt.Red_CustomInfo.ToArray(),
                            Red_Items: gpt.Red_Items.ProdVersion(),
                            Consel_MerchantPro: gpt.Consel_MerchantPro,
                            Consel_CustomerInfo: gpt.Consel_CustomerInfo.ProdVersion(),
                            payPalBillingAgreementDescription: gpt.payPalBillingAgreementDescription,
                            OrderDetails: gpt.OrderDetails.ProdVersion()
                        );
                        urlFormat = string.Format(Endpoints.ProdPayEntry, Endpoints.PaymentPage);
                    }
                }
            } catch (Exception ex) {
                //Log the error
                LocalizedString error = T("Request to GestPay service failed: {0}", ex.Message);
                Logger.Error(error.Text);
                //update the PaymentRecord for this transaction
                EndPayment(paymentId, false, null, error.Text);
                //return the URL of a suitable error page (call this.GetPaymentInfoUrl after inserting the error in the PaymentRecord)
                return GetPaymentInfoUrl(paymentId);
            }


            try {
                res = new EncryptDecryptTransactionResult(encryptXML);
                Validator.ValidateObject(res, new ValidationContext(res), true);
            } catch (Exception ex) {
                //Log the error
                Logger.Error(T("Validation problems on the response received: {0}", ex.Message).Text);
                //update the PaymentRecord for this transaction
                EndPayment(paymentId, false, null, T("Validation problems on the response received: {0}", ex.Message).Text);
                //return the URL of a suitable error page (call this.GetPaymentInfoUrl after inserting the error in the PaymentRecord)
                return GetPaymentInfoUrl(paymentId);
            }

            if (res.TransactionResult.ToUpperInvariant() == "OK") {
                return string.Format(urlFormat, settings.GestPayShopLogin, res.CryptDecryptString);
            } else {
                //Log the error
                LocalizedString error = T("Remote service replied with an error. Error {0}: {1}", res.ErrorCode, res.ErrorDescription);
                Logger.Error(error.Text);
                //update the PaymentRecord for this transaction
                EndPayment(paymentId, false, null, error.Text);
                //return the URL of a suitable error page (call this.GetPaymentInfoUrl after inserting the error in the PaymentRecord)
                return GetPaymentInfoUrl(paymentId);
            }
        }

        /// <summary>
        /// After receiving a call form the GestPay servers, we use this method to parse what we have been sent and 
        /// interpret it to an actual response.
        /// </summary>
        /// <param name="a">The GestPay Shop Login used in the transaction.</param>
        /// <param name="b">An encrypted string representing the transaction results.</param>
        public TransactionOutcome ReceiveS2STransaction(string a, string b) {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<PaymentGestPaySettingsPart>();

            if (a == settings.GestPayShopLogin) {
                //decrypt the string b
                XmlNode outcome;
                try {
                    outcome = Decrypt(a, b, settings.UseTestEnvironment);
                } catch (Exception ex) {
                    //Log the error
                    LocalizedString error = T("Request to GestPay service failed: {0}", ex.Message);
                    Logger.Error(error.Text);
                    throw new Exception(error.Text);
                }

                string errors = "";
                //this is the method that should handle the end of the transaction on our end.
                TransactionOutcome result = new TransactionOutcome();
                try {
                    result = new TransactionOutcome(outcome);
                    Validator.ValidateObject(result, new ValidationContext(result), true);
                } catch (Exception ex) {
                    LocalizedString exception = T("Validation problems on the response received: {0}", ex.Message);
                    //Log the error
                    Logger.Error(exception.Text);
                    errors += exception.Text + Environment.NewLine;

                    result.TransactionResult = "KO";
                    result.ErrorDescription = exception.Text;
                }

                //update the PaymentRecord for this transaction
                int pId;
                if (result != null && int.TryParse(result.ShopTransactionID, out pId)) {
                    //We can process things normally
                    try {
                        decimal amount = decimal.Parse(result.Amount, CultureInfo.InvariantCulture);
                        if (amount != GetPaymentInfo(pId).Amount) {
                            EndPayment(pId, false, null, 
                                T("The amount from the GestPay server ({0}) does not match the amount in our records ({1}).", 
                                    amount.ToString("0.##", CultureInfo.InvariantCulture), 
                                    GetPaymentInfo(pId).Amount.ToString("0.##", CultureInfo.InvariantCulture)).Text);
                        } else if (result.TransactionResult == "OK" && result.ErrorCode == "0") {
                            EndPayment(pId, true, null, null, string.Format("Bank ID: {0}\nAuth Code: {1}", result.BankTransactionID, result.AuthorizationCode));
                        } else {
                            result.TransactionResult = "KO";
                            EndPayment(pId, false, result.ErrorCode, result.ErrorDescription);
                        }
                    } catch (Exception exin) {
                        result.TransactionResult = "KO";
                        errors += exin.Message + Environment.NewLine;
                        throw new Exception(T("EndPayment caused exception: {0}.\npId: {1}\nerror:{2}", exin.Message, pId.ToString(), errors).Text);
                    }
                } else {
                    result.TransactionResult = "KO";
                    errors = T("Failed to identify transaction from GestPay's response. Additional error: {0}", errors).Text;
                    Logger.Error(errors);
                }

                result.ErrorDescription = errors;

                return result;
            }

            LocalizedString ErrorHere = T("GestPay sent transaction information, but the Shop Login was wrong ({0})", a);
            Logger.Error(ErrorHere.Text);
            return TransactionOutcome.InternalError(ErrorHere.Text);
        }
        /// <summary>
        /// Calls GestPay's decryption service to decode the information we received.
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="cryptedInfo"></param>
        /// <param name="testEnvironmanet"></param>
        /// <returns></returns>
        private XmlNode Decrypt(string shop, string cryptedInfo, bool testEnvironmanet) {
            XmlNode outcome;
            if (testEnvironmanet) {
                string endpoint = string.Format(Endpoints.TestWSEntry, Endpoints.CryptDecryptEndPoint);
                endpoint = endpoint.Substring(0, endpoint.Length - 4);
                BasicHttpBinding binding = new BasicHttpBinding();
                endpoint = Regex.Replace(endpoint, "(https)", "http"); //https gives errors
                EndpointAddress address = new EndpointAddress(endpoint);
                using (var client = new CryptDecryptTest.WSCryptDecryptSoapClient(binding, address)) {
                    outcome = client.Decrypt(shop, cryptedInfo);
                }
            } else {
                string endpoint = string.Format(Endpoints.ProdWSEntry, Endpoints.CryptDecryptEndPoint);
                endpoint = endpoint.Substring(0, endpoint.Length - 4);
                BasicHttpBinding binding = new BasicHttpBinding();
                endpoint = Regex.Replace(endpoint, "(https)", "http"); //https gives errors
                EndpointAddress address = new EndpointAddress(endpoint);
                using (var client = new CryptDecryptProd.WSCryptDecryptSoapClient(binding, address)) {
                    outcome = client.Decrypt(shop, cryptedInfo);
                }
            }
            return outcome;
        }
        /// <summary>
        /// Given the results of a transaction, get the url of the page showing them to the user. This url, depending on the call that started the
        /// transaction, may not actually be an url.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public string InterpretTransactionResult(string a, string b) {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<PaymentGestPaySettingsPart>();

            if (a == settings.GestPayShopLogin) {

                XmlNode outcome;// = Decrypt(a, b, settings.UseTestEnvironment);
                try {
                    outcome = Decrypt(a, b, settings.UseTestEnvironment);
                } catch (Exception ex) {
                    //Log the error
                    LocalizedString error = T("Request to GestPay service failed: {0}", ex.Message);
                    Logger.Error(error.Text);
                    throw new Exception(error.Text);
                }

                //the closure of the transaction should be handled in the S2S method
                //Here, eventually, we send error messages or info strings, that wee add to the record in the endpayment
                TransactionOutcome result = new TransactionOutcome();
                try {
                    result = new TransactionOutcome(outcome);
                    Validator.ValidateObject(result, new ValidationContext(result), true);
                } catch (Exception ex) {
                    LocalizedString exception = T("Validation problems on the response received: {0}", ex.Message);
                    //Log the error
                    Logger.Error(exception.Text);
                    throw new Exception(exception.Text);
                    ////update the PaymentRecord for this transaction
                    //int pId;
                    //if (result != null && int.TryParse(result.ShopTransactionID, out pId)) {
                    //    try {
                    //        EndPayment(pId, false, null, exception.Text);
                    //    } catch (Exception exin) {
                    //        throw new Exception(T("EndPayment caused exception: {0}.\npId: {1}\nsuccess: false\nerror:{2}", exin.Message, pId.ToString(), exception.Text).Text);
                    //    }
                    //    return GetPaymentInfoUrl(pId);
                    //} else {
                    //    exception = T("Failed to identify transaction from GestPay's response. Additional error: {0}", exception.Text);
                    //    Logger.Error(exception.Text);
                    //    throw new Exception(exception.Text);
                    //}
                }

                int paymentId;
                if (int.TryParse(result.ShopTransactionID, out paymentId)) {
                    //if (result.TransactionResult == "OK") {
                    //    try {
                    //        EndPayment(paymentId, true, null, null);
                    //    } catch (Exception exin) {
                    //        throw new Exception(T("EndPayment caused exception: {0}.\npId: {1}\nsuccess: true", exin.Message, paymentId.ToString()).Text);
                    //    }
                    //} else {
                    //    try {
                    //        EndPayment(paymentId, false, result.ErrorCode, result.ErrorDescription);
                    //    } catch (Exception exin) {
                    //        throw new Exception(T("EndPayment caused exception: {0}.\npId: {1}\nsuccess: false\nerror:{2}\ninfo:{3}", exin.Message, paymentId.ToString(), result.ErrorCode, result.ErrorDescription).Text);
                    //    }
                    //}
                    return GetPaymentInfoUrl(paymentId);
                } else {
                    if (result.TransactionResult == "KO") {
                        throw new Exception(T("Failed to get the transaction Id back from GestPay. Error {0}: {1}", result.ErrorCode, result.ErrorDescription).Text);
                    }
                    throw new Exception(T("Failed to get the transaction Id back from GestPay.").Text);
                }
            }

            LocalizedString ErrorHere = T("GestPay sent transaction information, but the Shop Login was wrong ({0})", a);
            Logger.Error(ErrorHere.Text);
            throw new Exception(ErrorHere.Text);
        }
    }
}