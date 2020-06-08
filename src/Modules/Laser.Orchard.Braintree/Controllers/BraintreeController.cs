using Laser.Orchard.Braintree.Models;
using Laser.Orchard.Braintree.Services;
using Laser.Orchard.Braintree.ViewModels;
using Laser.Orchard.PaymentGateway;
using Laser.Orchard.PaymentGateway.Models;
using Newtonsoft;
using Newtonsoft.Json;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Braintree.Controllers {
    public class BraintreeController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly BraintreePosService _posService;
        private readonly IBraintreeService _braintreeService;

        public BraintreeController(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler, IBraintreeService braintreeService) {
            _orchardServices = orchardServices;
            _posService = new BraintreePosService(orchardServices, repository, paymentEventHandler);
            _braintreeService = braintreeService;

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
            } else {
                payment = _posService.GetPaymentInfo(guid);
            }
            pid = payment.Id;
            var settings = _orchardServices.WorkContext.CurrentSite.As<BraintreeSiteSettingsPart>();
            if (settings.CurrencyCode != payment.Currency) {
                //throw new Exception(string.Format("Invalid currency code. Valid currency is {0}.", settings.CurrencyCode));
                string error = string.Format("Invalid currency code. Valid currency is {0}.", settings.CurrencyCode);
                _posService.EndPayment(payment.Id, false, error, error);
                return Redirect(_posService.GetPaymentInfoUrl(payment.Id));
            }
            PaymentVM model = new PaymentVM();
            model.Record = payment;
            model.TenantBaseUrl = Url.Action("Index").Replace("/Laser.Orchard.Braintree/Braintree", "");
            return View("Index", model);
        }

        /// <summary>
        /// Entry point per dispositivi mobile per iniziare il pagamento tramite Braintree.
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetTokenAndPid(string reason, decimal amount, string currency, int itemId = 0) {
            var clientToken = _braintreeService.GetClientToken();
            var payment = new PaymentRecord {
                Reason = reason,
                Amount = amount,
                Currency = currency,
                ContentItemId = itemId
            };
            payment = _posService.StartPayment(payment);
            var result = new { Token = clientToken, Pid = payment.Id };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private PaymentResult FinalizePayment(string nonce, string sPid) {
            int pid = int.Parse(sPid);
            var paymentInfo = _posService.GetPaymentInfo(pid);
            var payResult = _braintreeService.Pay(new PaymentContext(nonce, paymentInfo));
            string error = "";
            string transactionId = "";
            string info = JsonConvert.SerializeObject(payResult);
            if (payResult.Success == false) {
                // use dictionary for manage error payment 
                // this values are present in table of Braintree
                // errors that are not handled will go to the error log so you can view and manage them
                //error = payResult.ResponseText;
                var braintreeError = GetBraintreeError();
                if (!string.IsNullOrWhiteSpace(payResult.ResponseCode) && braintreeError.ContainsKey(payResult.ResponseCode)) {
                    error = braintreeError.FirstOrDefault(er => er.Key == payResult.ResponseCode).Value;
                    Logger.Information(string.Format(@"Error on payment for order {0}: Error {1}. Details: {2}",
                       paymentInfo.ContentItemId,
                       payResult.ResponseText,
                       info));
                }
                else {
                    // check is number
                    int errCode;
                    if (!int.TryParse(payResult.ResponseCode, out errCode)) {
                        errCode = -1;
                    }
                    // this range of errors has same error
                    // this default value are in table of errors of Braintree
                    if(errCode >= 2101 && errCode <= 2999) {
                        error = T("The customer's bank is unwilling to accept the transaction. The customer will need to contact their bank for more details regarding this generic decline.").Text;
                        Logger.Information(string.Format(@"Error on payment for order {0}: Error {1}. Details: {2}",
                          paymentInfo.ContentItemId,
                          payResult.ResponseText,
                          info));
                    }
                    else {
                        error = T("Transaction failed. The customer will have to try again to make the payment").Text;
                        Logger.Error(string.Format(@"Error on payment for order {0}: Error {1}. Details: {2}",
                           paymentInfo.ContentItemId,
                           payResult.ResponseText,
                           info));
                    }
                }
            }
            // even failed transactions may be recorded
            transactionId = payResult?.TransactionId ?? "";
            _posService.EndPayment(pid, payResult.Success, error, info, transactionId);
            return new PaymentResult { Pid = pid, Success = payResult.Success, Error = error, TransactionId = transactionId};
        }
        [HttpPost]
        public ActionResult PayMobile(MobilePay model) {
            string nonce = model.payment_method_nonce;
            string sPid = model.pid;
            var outcome = FinalizePayment(nonce, sPid);
            return Json(outcome);
        }
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetToken() {
            var clientToken = _braintreeService.GetClientToken();
            return Content(clientToken, "text/plain", Encoding.UTF8);
        }

        [Themed]
        [HttpPost]
        public ActionResult Pay() {
            string nonce = Request["payment_method_nonce"];
            string sPid = Request["pid"];
            var outcome = FinalizePayment(nonce, sPid);
            return Redirect(_posService.GetPaymentInfoUrl(outcome.Pid));
        }

        public class MobilePay {
            public string payment_method_nonce { get; set; }
            public string pid { get; set; }
        }

        private class PaymentResult {
            public int Pid { get; set; }
            public bool Success { get; set; }
            public string Error { get; set; }
            public string TransactionId { get; set; }
        }

        private Dictionary<string, string> GetBraintreeError() {
            // error code Braintree, message error
            return new Dictionary<string, string> {
                {"2000",T("The customer's bank is unwilling to accept the transaction. The customer will need to contact their bank for more details.").Text},
                {"2001",T("The account did not have sufficient funds to cover the transaction amount at the time of the transaction").Text},
                {"2002",T("The attempted transaction exceeds the withdrawal limit of the account. The customer will need to contact their bank to change the account limits or use a different payment method.").Text},
                {"2003",T("The attempted transaction exceeds the activity limit of the account. The customer will need to contact their bank to change the account limits or use a different payment method.").Text},
                {"2004",T("Card is expired. The customer will need to use a different payment method.").Text},
                {"2005",T("The customer entered an invalid payment method or made a typo in their credit card information. Have the customer correct their payment information and attempt the transaction again – if the decline persists, they will need to contact their bank.").Text},
                {"2006",T("The customer entered an invalid payment method or made a typo in their card expiration date. Have the customer correct their payment information and attempt the transaction again – if the decline persists, they will need to contact their bank.").Text},
                {"2007",T("The submitted card number is not on file with the card-issuing bank. The customer will need to contact their bank.").Text},
                {"2008",T("The submitted card number does not include the proper number of digits. Have the customer attempt the transaction again – if the decline persists, the customer will need to contact their bank.").Text},
                {"2009",T("This decline code could indicate that the submitted card number does not correlate to an existing card-issuing bank or that there is a connectivity error with the issuer. The customer will need to contact their bank for more information.").Text},
                {"2010",T("The customer entered in an invalid security code or made a typo in their card information. Have the customer attempt the transaction again – if the decline persists, the customer will need to contact their bank.").Text},
                {"2011",T("The customer’s bank is requesting that the merchant (you) call to obtain a special authorization code in order to complete this transaction. This can result in a lengthy process – we recommend obtaining a new payment method instead.").Text},
                {"2012",T("The card used has likely been reported as lost. The customer will need to contact their bank for more information.").Text},
                {"2013",T("The card used has likely been reported as stolen. The customer will need to contact their bank for more information.").Text},
                {"2014",T("The customer’s bank suspects fraud – they will need to contact their bank for more information.").Text},
                {"2015",T("The customer's bank is declining the transaction for unspecified reasons, possibly due to an issue with the card itself. They will need to contact their bank or use a different payment method.").Text},
                {"2016",T("The submitted transaction appears to be a duplicate of a previously submitted transaction and was declined to prevent charging the same card twice for the same service.").Text},
                {"2017",T("The customer requested a cancellation of a single transaction – reach out to them for more information.").Text},
                {"2018",T("The customer requested the cancellation of a recurring transaction or subscription – reach out to them for more information.").Text},
                {"2019",T("The customer’s bank declined the transaction, typically because the card in question does not support this type of transaction – for example, the customer used an FSA debit card for a non-healthcare related purchase. They will need to contact their bank for more information.").Text},
                {"2020",T("The customer will need to contact their bank for more information.").Text},
                {"2021",T("The customer's bank is declining the transaction, possibly due to a fraud concern. They will need to contact their bank or use a different payment method.").Text},
                {"2022",T("The submitted card has expired or been reported lost and a new card has been issued. Reach out to your customer to obtain updated card information.").Text },
                {"2023",T("Your account can't process transactions with the intended feature – for example, 3D Secure or Level 2/Level 3 data. If you believe your merchant account should be set up to accept this type of transaction.").Text},
                {"2024",T("Your account can't process the attempted card type. If you believe your merchant account should be set up to accept this type of card.").Text},
                {"2025",T("Depending on your region, this response could indicate a connectivity or setup issue.").Text},
                {"2026",T("The customer’s bank declined the transaction, typically because the card in question does not support this type of transaction. If this response persists across transactions for multiple customers, it could indicate a connectivity or setup issue.").Text},
                {"2027",T("This rare decline code indicates an issue with processing the amount of the transaction. The customer will need to contact their bank for more details.").Text},
                {"2028",T("There is a setup issue with your account.").Text},
                {"2029",T("This response generally indicates that there is a problem with the submitted card. The customer will need to use a different payment method.").Text},
                {"2030",T("There is a setup issue with your account.").Text},
                {"2031",T("The cardholder’s bank does not support $0.00 card verifications. Enable the Retry All Failed $0 option to resolve this error.").Text},
                {"2032",T("Surcharge amount not permitted on this card. The customer will need to use a different payment method.").Text},
                {"2033",T("An error occurred when communicating with the processor. The customer will need to contact their bank for more details.").Text},
                {"2034",T("An error occurred and the intended transaction was not completed. Attempt the transaction again.").Text},
                {"2035",T("The customer's bank approved the transaction for less than the requested amount. Have the customer attempt the transaction again – if the decline persists, the customer will need to use a different payment method.").Text},
                {"2036",T("An error occurred when trying to process the authorization. This response could indicate an issue with the customer’s card or that the processor doesn't allow this action.").Text},
                {"2037",T("The indicated authorization has already been reversed. If you believe this to be false.").Text},
                {"2038",T("The customer's bank is unwilling to accept the transaction. The reasons for this response can vary – customer will need to contact their bank for more details.").Text},
                {"2039",T("The authorization code was not found or not provided. Have the customer attempt the transaction again – if the decline persists, they will need to contact their bank.").Text},
                {"2040",T("There may be an issue with the configuration of your account. Have the customer attempt the transaction again – if the decline persists.").Text},
                {"2041",T("The card used for this transaction requires customer approval – they will need to contact their bank.").Text},
                {"2042",T("There may be an issue with the configuration of your account. Have the customer attempt the transaction again – if the decline persists.").Text},
                {"2043",T("The card-issuing bank will not allow this transaction. The customer will need to contact their bank for more information.").Text},
                {"2044",T("The card-issuing bank has declined this transaction. Have the customer attempt the transaction again – if the decline persists, they will need to contact their bank for more information.").Text},
                {"2045",T("There is a setup issue with your account.").Text},
                {"2046",T("The customer's bank is unwilling to accept the transaction. For credit/debit card transactions, the customer will need to contact their bank for more details regarding this generic decline; if this is a PayPal transaction, the customer will need to contact PayPal.").Text},
                {"2047",T("The customer’s card has been reported as lost or stolen by the cardholder and the card-issuing bank has requested that merchants keep the card and call the number on the back to report it. As an online merchant, you don’t have the physical card and can't complete this request – obtain a different payment method from the customer.").Text},
                {"2048",T("The authorized amount is set to zero, is unreadable, or exceeds the allowable amount. Make sure the amount is greater than zero and in a suitable format.").Text},
                {"2049",T("A non-numeric value was sent with the attempted transaction. Fix errors and resubmit with the transaction with the proper SKU Number.").Text},
                {"2050",T("There may be an issue with the customer’s card or a temporary issue at the card-issuing bank. The customer will need to contact their bank for more information or use a different payment method.").Text},
                {"2051",T("There may be an issue with the customer’s credit card or a temporary issue at the card-issuing bank. Have the customer attempt the transaction again – if the decline persists, ask for a different card or payment method.").Text},
                {"2053",T("The card used was reported lost or stolen. The customer will need to contact their bank for more information or use a different payment method.").Text},
                {"2054",T("Either the refund amount is greater than the original transaction or the card-issuing bank does not allow partial refunds. The customer will need to contact their bank for more information or use a different payment method.").Text},
                {"2055",T("Invalid Transaction Division Number.").Text},
                {"2056",T("Transaction amount exceeds the transaction division limit.").Text},
                {"2057",T("The customer will need to contact their issuing bank for more information.").Text},
                {"2058",T("The attempted card can't be processed without enabling 3D Secure for your account.").Text},
                {"2059",T("PayPal was unable to verify that the transaction qualifies for Seller Protection because the address was improperly formatted. The customer should contact PayPal for more information or use a different payment method.").Text},
                {"2060",T("Both the AVS and CVV checks failed for this transaction. The customer should contact PayPal for more information or use a different payment method.").Text},
                {"2061",T("There may be an issue with the customer’s card or a temporary issue at the card-issuing bank. Have the customer attempt the transaction again – if the decline persists, ask for a different card or payment method.").Text},
                {"2062",T("There may be an issue with the customer’s card or a temporary issue at the card-issuing bank. Have the customer attempt the transaction again – if the decline persists, ask for a different card or payment method.").Text},
                {"2063",T("You can't process this transaction because your account is set to block certain payment types, such as eChecks or foreign currencies. If you believe you have received this decline in error.").Text},
                {"2064",T("There may be an issue with the configuration of your account for the currency specified.").Text},
                {"2065",T("PayPal requires that refunds are issued within 180 days of the sale. This refund can't be successfully processed.").Text},
                {"2066",T("Contact PayPal’s Support team to resolve this issue with your account. Then, you can attempt the transaction again.").Text},
                {"2067",T("The PayPal authorization is no longer valid.").Text},
                {"2068",T("You'll need to contact PayPal’s Support team to resolve an issue with your account. Once resolved, you can attempt to process the transaction again.").Text},
                {"2069",T("The submitted PayPal transaction appears to be a duplicate of a previously submitted transaction. This decline code indicates an attempt to prevent charging the same PayPal account twice for the same service.").Text},
                {"2070",T("The customer revoked authorization for this payment method. Reach out to the customer for more information or a different payment method.").Text},
                {"2071",T("Customer has not finalized setup of their PayPal account. Reach out to the customer for more information or a different payment method.").Text},
                {"2072",T("Customer made a typo or is attempting to use an invalid PayPal account.").Text},
                {"2073",T("PayPal can't validate this transaction.").Text},
                {"2074",T("The customer’s payment method associated with their PayPal account was declined. Reach out to the customer for more information or a different payment method.").Text},
                {"2075",T("The customer’s PayPal account can't be used for transactions at this time. The customer will need to contact PayPal for more information or use a different payment method.").Text},
                {"2076",T("The customer should contact PayPal for more information or use a different payment method. You may also receive this response if you create transactions using the email address registered with your PayPal Business Account.").Text},
                {"2077",T("PayPal has declined this transaction due to risk limitations. You'll need to contact PayPal’s Support team to resolve this issue.").Text},
                {"2079",T("You'll need to contact us to resolve an issue with your account. Once resolved, you can attempt to process the transaction again.").Text},
                {"2081",T("Braintree received an unsupported Pending Verification response from PayPal. Contact Braintree’s Support team to resolve a potential issue with your account settings. If there is no issue with your account, have the customer reach out to PayPal for more information.").Text},
                {"2082",T("This transaction requires the customer to be a resident of the same country as the merchant. Reach out to the customer for a different payment method.").Text},
                {"2083",T("This transaction requires the payer to provide a valid phone number. The customer should contact PayPal for more information or use a different payment method.").Text},
                {"2084",T("The customer must complete their PayPal account information, including submitting their phone number and all required tax information.").Text},
                {"2085",T("Fraud settings on your PayPal business account are blocking payments from this customer. These can be adjusted in the Block Payments section of your PayPal business account.").Text},
                {"2086",T("The settings on the customer's account do not allow a transaction amount this large. They will need to contact PayPal to resolve this issue.").Text},
                {"2087",T("PayPal API permissions are not set up to allow reference transactions. You'll need to contact PayPal’s Support team to resolve an issue with your account. Once resolved, you can attempt to process the transaction again.").Text},
                {"2088",T("This currency is not currently supported by your PayPal account. You can accept additional currencies by updating your PayPal profile.").Text},
                {"2089",T("PayPal API permissions are not set up between your PayPal business accounts.").Text},
                {"2090",T("Your PayPal account is not set up to refund amounts higher than the original transaction amount. Contact PayPal's Support team for information on how to enable this.").Text},
                {"2091",T("Your PayPal account can only process transactions in the currency of your home country. Contact PayPal's Support team for more information.").Text},
                {"2092",T("The processor is unable to provide a definitive answer about the customer's bank account. Please try a different US bank account verification method.").Text},
                {"2093",T("The PayPal payment method has either expired or been canceled.").Text},
                {"2094",T("Your integration is likely making PayPal calls out of sequence. Check the error response for more details.").Text},
                {"2095",T("Once a PayPal transaction is partially refunded, all subsequent refunds must also be partial refunds for the remaining amount or less. Full refunds are not allowed after a PayPal transaction has been partially refunded.").Text},
                {"2096",T("PayPal buyer account can't be the same as the seller account.").Text},
                {"2097",T("PayPal authorization amount is greater than the allowed limit on the order.").Text},
                {"2098",T("The number of PayPal authorizations is greater than the allowed number on the order.").Text},
                {"2099",T("The customer's bank declined the transaction because a 3D Secure authentication was not performed during checkout. Have the customer authenticate using 3D Secure, then attempt the authorization again.").Text},
                {"2100",T("Your PayPal permissions are not set up to allow channel initiated billing transactions. Contact PayPal's Support team for information on how to enable this. Once resolved, you can attempt to process the transaction again.").Text},
                {"3000",T("Processor Network Unavailable – Try Again").Text}
              };
        }
    }
}