using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Controllers {
    [WebApiKeyFilter(true)]
    public class PaymentGatewayAPIController : ApiController {

        private readonly IEnumerable<IPosService> _posServices;

        public Localizer T { get; set; }

        public PaymentGatewayAPIController(IEnumerable<IPosService> posServices) {
            _posServices = posServices;

            T = NullLocalizer.Instance;
        }

        /// <summary>
        /// endpoint: /API/Laser.Orchard.PaymentGateway/PaymentGatewayAPI/GetPosNames
        /// A call to this endpoint, with no parameters, returns the list of the names of available payment methods.
        /// Example response:
        /// {
        ///  "ErrorCode": 0,
        ///  "ResolutionAction": 0,
        ///  "Success": true,
        ///  "Message": "",
        ///  "Data": {
        ///    "posNames": [
        ///      "Braintree and PayPal",
        ///      "CartaSì X-Pay",
        ///      "GestPay"
        ///    ]
        ///  }
        /// }
        /// </summary>
        /// <returns>A response whose Data field contains an array called posNames that contains the names to be used when referring
        /// to the payment methods in the other API calls.</returns>
        public PaymentGatewayResponse GetPosNames() {
            PaymentGatewayResponse res = new PaymentGatewayResponse() {
                Success = true,
                ErrorCode = PaymentGatewayErrorCode.NoError,
                Data = new { posNames = AllPosNames() },
                Message = ""
            };
            return res;
        }

        /// <summary>
        /// endpoint: /API/Laser.Orchard.PaymentGateway/PaymentGatewayAPI/GetValidCurrencies
        /// A call to this endpoint returns the list of the currencies that may be used with a given payment method.
        /// Example response:
        /// {
        ///  "ErrorCode": 0,
        ///  "ResolutionAction": 0,
        ///  "Success": true,
        ///  "Message": "",
        ///  "Data": {
        ///    "validCurrencies": [
        ///      "EUR"
        ///    ]
        ///  }
        /// }
        /// </summary>
        /// <param name="posName">A string representing the name of the pos, as given by a call to GetPosNames. This parameter is mandatory.</param>
        /// <returns>A response whose Data field contains an array called validCurrencies that contains the valid currency string that may be used with the pos.</returns>
        public PaymentGatewayResponse GetValidCurrencies(string posName) {
            var vc = ValidCurrencies(posName);
            if (vc == null || vc.Count == 0) {
                return new PaymentGatewayResponse() {
                    Success = false,
                    Message = T("There are no valid currencies for the POS, or the pos is not valid. See this response's data object for a list of valid POS names").Text,
                    Data = new { posNames = AllPosNames() },
                    ErrorCode = PaymentGatewayErrorCode.PosNotFound,
                    ResolutionAction = PaymentGatewayResolutionAction.UpdatePosNames
                };
            }
            return new PaymentGatewayResponse() {
                Success = true,
                Message = "",
                Data = new { validCurrencies = vc },
                ErrorCode = PaymentGatewayErrorCode.NoError,
                ResolutionAction = PaymentGatewayResolutionAction.NoAction
            };
        }

        /// <summary>
        /// endpoint: /API/Laser.Orchard.PaymentGateway/PaymentGatewayAPI/GetAPIFilterTerms
        /// A call to this endpoint returns the list valid values for the API json filter to be applied in the transaction responses.
        /// Example response:
        /// {
        ///  "ErrorCode": 0,
        ///  "ResolutionAction": 0,
        ///  "Success": true,
        ///  "Message": "The Data object contains the array of valid filter parameters. These are case-insensitive."",
        ///  "Data": {
        ///    "validTerms": [
        ///      "REASON",
        ///      "POSNAME",
        ///      "AMOUNT",
        ///      "CURRENCY",
        ///      "ERROR",
        ///      "INFO",
        ///      "USERID"
        ///    ]
        ///  }
        /// }
        /// </summary>
        /// <returns>A response whose Data field contains an array called validTerms that contains the valid strings that may be passed as optional json filters.</returns>
        public PaymentGatewayResponse GetAPIFilterTerms() {
            return new PaymentGatewayResponse() {
                Success = true,
                Message = T("The Data object contains the array of valid filter parameters. These are case-insensitive.").Text,
                Data = new { validTerms = PaymentRecord.ValidAPIFilters },
                ErrorCode = PaymentGatewayErrorCode.NoError,
                ResolutionAction = PaymentGatewayResolutionAction.NoAction
            };
        }

        /// <summary>
        /// endpoint: /API/Laser.Orchard.PaymentGateway/PaymentGatewayAPI/GetVirtualPosUrl
        /// Get the Url of the virtual pos, based on the parameters passed in the call.
        /// Example response:
        /// {
        ///  "ErrorCode": 0,
        ///  "ResolutionAction": 0,
        ///  "Success": true,
        ///  "Message": "The Data object contains the array of valid filter parameters. These are case-insensitive."",
        ///  "Data": {
        ///    "redirectUrl": "https://testecomm.sella.it/pagam/pagam.aspx?a=GESPAY63353&b=JEkIJQMBqbRabcJwzbGRHI1q8*dZOkTVfBzFjl0ciCaOrUZpYxTgDgDAG_SIC_Uy1uUBChnOEqs0fmXc2K9WTgsGDi*qlTQa*A_Nq*F2ylc"
        ///  }
        /// }
        /// </summary>
        /// <param name="posName">Mandatory: Name of the payment gateway whose POS we are trying to reach.</param>
        /// <param name="amount">Mandatory: Amount to be payed.</param>
        /// <param name="currency">Mandatory: Currency of payment.</param>
        /// <param name="itemId">Optional: Id of Content Item associated with payment.</param>
        /// <param name="reason">Optional: Description of reason for payment.</param>
        /// <param name="redirectUrl">Optional: Url to which we want to redirect the browser from the Action handling the end of
        /// the transaction.</param>
        /// <param name="schema">Optional: Schema for the redirect from the Action handling the end of the transaction.</param>
        /// <param name="filters">Optional: Comma-separated list of parameters for the json filter of the results. The valid strings are given by a call to GetAPIFilterTerms.
        /// The response received at the end of the transaction will contain the requested parameters in addition to the default ones.</param>
        /// <returns>A response whose Data field contains a redirectUrl string telling where the application should redirect the user for payment,
        /// or an error with aditional information.</returns>
        public PaymentGatewayResponse GetVirtualPosUrl(
            string posName, decimal amount, string currency,
            int? itemId = 0, string reason = "", string redirectUrl = "", string schema = "", string filters = "") {
            bool success = false;
            string msg = "";
            dynamic data = new System.Dynamic.ExpandoObject();
            PaymentGatewayErrorCode error = PaymentGatewayErrorCode.NoError;
            PaymentGatewayResolutionAction action = PaymentGatewayResolutionAction.NoAction;
            //get pos from posName
            var pos = _posServices.Where(ps => ps.GetPosName() == posName).SingleOrDefault();
            if (pos == null) {
                //ERROR: no pos with that name
                success = false;
                error = PaymentGatewayErrorCode.PosNotFound;
                action = PaymentGatewayResolutionAction.UpdatePosNames;
                data.posNames = AllPosNames();
                msg = T("Could not find a POS called \"{0}\": you may find a list of available POS names in this response's data object.", posName).Text;
            } else {
                //check whether currency is valid
                var vc = ValidCurrencies(pos);
                if (string.IsNullOrWhiteSpace(currency) || !vc.Contains(currency)) {
                    //currency is required
                    success = false;
                    error = PaymentGatewayErrorCode.InvalidCurrency;
                    action = PaymentGatewayResolutionAction.VerifyInformation;
                    data.validCurrencies = vc;
                    msg = T("A valid currency is required. You may find a list of valid currencies in this response's data object.").Text;
                } else {
                    //create PaymentRecord (using startPayment)
                    PaymentRecord record = null;
                    try {
                        //create a string for the apifilters, keeping only the valid ones, and removing duplicates
                        filters = string.Join(",",
                            filters.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                            .Where(s => PaymentRecord.IsValidAPIFilter(s))
                            .Distinct(StringComparer.InvariantCultureIgnoreCase)
                            .ToList());
                        record = pos.StartPayment(new PaymentRecord() {
                            Reason = reason,
                            Amount = amount,
                            Currency = currency,
                            ContentItemId = itemId.Value,
                            CustomRedirectUrl = redirectUrl,
                            CustomRedirectSchema = schema,
                            APIFilters = filters
                        });
                    } catch (Exception ex) {
                        success = false;
                        error = PaymentGatewayErrorCode.ImpossibleToCreateRecord;
                        action = PaymentGatewayResolutionAction.VerifyInformation;
                        msg = ex.Message;
                    }
                    int paymentId = record.Id;
                    //get the redirect url for the pos
                    try {
                        data.redirectUrl = pos.GetPosUrl(paymentId);
                        success = true;
                    } catch (Exception ex) {
                        //some payment services may not return a redirect url (e.g. Braintree)
                        //handle this case with an error
                        success = false;
                        msg = ex.Message;
                        error = PaymentGatewayErrorCode.CallNotValid;
                        action = PaymentGatewayResolutionAction.DoSomethingElse;
                    }
                }
            }

            return new PaymentGatewayResponse() {
                Success = success,
                Message = msg,
                Data = data,
                ErrorCode = error,
                ResolutionAction = action
            };
        }

        #region Private methods
        private List<string> AllPosNames() {
            List<string> posNames = new List<string>();
            foreach (var service in _posServices) {
                posNames.Add(service.GetPosName());
            }
            return posNames;
        }
        private List<string> ValidCurrencies(string posName) {
            //get the pos from the posName
            var pos = _posServices.Where(ps => ps.GetPosName() == posName).SingleOrDefault();
            if (pos == null)
                return null;
            return ValidCurrencies(pos);
        }
        private List<string> ValidCurrencies(IPosService pos) {
            return pos.GetAllValidCurrencies();
        }
        #endregion

        #region Responses
        //we extend Laser.Orchard.StartupConfig.ViewModels.Response for error codes specific to PaymentGateway
        public enum PaymentGatewayErrorCode {
            NoError = 0, GenericError = 1,
            PosNotFound = 5001, //No POS was found with the given name
            InvalidCurrency = 5002, //the selected POS does not support this currency
            ImpossibleToCreateRecord = 5003, //attempt to create the record failed
            CallNotValid = 5004 //this call is not valid for the selected POS
        }
        public enum PaymentGatewayResolutionAction {
            NoAction = 0,
            TryAgain = 5001, //it might have been a temporary error
            UpdatePosNames = 5002, //Update the list of available pos names
            VerifyInformation = 5003, //some parameter was not valid. 
            DoSomethingElse = 5004 //For the selected POS, you should be doing something else, rather than the call you made
        }
        public class PaymentGatewayResponse : Response {
            new public PaymentGatewayErrorCode ErrorCode { get; set; }
            new public PaymentGatewayResolutionAction ResolutionAction { get; set; }

            public PaymentGatewayResponse() {
                this.ErrorCode = PaymentGatewayErrorCode.GenericError;
                this.Success = false;
                this.Message = "Generic Error";
                this.ResolutionAction = PaymentGatewayResolutionAction.NoAction;
            }
        }
        #endregion
    }
}