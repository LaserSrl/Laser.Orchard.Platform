using Laser.Orchard.Braintree.Models;
using Laser.Orchard.PaymentGateway.Models;
using Newtonsoft.Json;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bt = Braintree;


namespace Laser.Orchard.Braintree.Services {
    public class DefaultBraintreeService : IBraintreeService {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;

        public DefaultBraintreeService(
            IOrchardServices orchardServices,
            IContentManager contentManager) {

            _orchardServices = orchardServices;
            _contentManager = contentManager;
        }

        public string GetClientToken() {
            var gateway = GetGateway();
            var clientToken = gateway.ClientToken.generate();
            return clientToken;
        }

        public TransactionResult Pay(
            string paymentMethodNonce,
            decimal amount,
            Dictionary<string, string> customFields) {
            var config = _orchardServices.WorkContext
                .CurrentSite.As<BraintreeSiteSettingsPart>();
            string merchant = config?.MerchantAccountId;
            TransactionResult result = new TransactionResult();
            var request = new Bt.TransactionRequest {
                Amount = amount,
                PaymentMethodNonce = paymentMethodNonce,
                MerchantAccountId = merchant
            };
            if (config.AutomaticPayment) {
                request.Options = new Bt.TransactionOptionsRequest {
                    SubmitForSettlement = config.AutomaticPayment
                };
            }
            if (customFields != null) {
                request.CustomFields = customFields;
            }
            var gateway = GetGateway();
            Bt.Result<Bt.Transaction> payResult = gateway.Transaction.Sale(request);
            Bt.Transaction tran = null;
            result.Success = payResult.IsSuccess();
            if (payResult.Target != null) {
                // caso success == true
                tran = payResult.Target;
            } else if (payResult.Transaction != null) {
                // caso success == false
                tran = payResult.Transaction;
            }
            if (tran != null) {
                result.Amount = tran.Amount.Value;
                result.AuthorizationCode = tran.ProcessorAuthorizationCode;
                if (tran.BillingAddress != null) {
                    result.BillingAddress = string.Format("{0} - {1} {2} - {3} {4} - {5} {6} {7} - {8}",
                        tran.BillingAddress.Company,
                        tran.BillingAddress.FirstName,
                        tran.BillingAddress.LastName,
                        tran.BillingAddress.StreetAddress,
                        tran.BillingAddress.ExtendedAddress,
                        tran.BillingAddress.PostalCode,
                        tran.BillingAddress.Locality,
                        tran.BillingAddress.Region,
                        tran.BillingAddress.CountryName);
                }
                result.CurrencyIsoCode = tran.CurrencyIsoCode;
                if (tran.Customer != null) {
                    result.Customer = string.Format("{0} {1}, {2}, ({3})",
                        tran.Customer.FirstName,
                        tran.Customer.LastName,
                        tran.Customer.Company,
                        tran.Customer.Email);
                }
                result.MerchantAccountId = tran.MerchantAccountId;
                result.OrderId = tran.OrderId;
                result.PurchaseOrderNumber = tran.PurchaseOrderNumber;
                result.ResponseCode = tran.ProcessorResponseCode;
                result.ResponseText = tran.ProcessorResponseText;
                result.Status = tran.Status.ToString();
                result.TransactionId = tran.Id;
                result.Type = tran.Type.ToString();
            }
            result.Details = JsonConvert.SerializeObject(payResult);
            return result;
        }

        public TransactionResult Pay(PaymentContext context) {
            var config = _orchardServices.WorkContext
                .CurrentSite.As<BraintreeSiteSettingsPart>();
            string merchant = config?.MerchantAccountId;
            TransactionResult result = new TransactionResult();
            var request = new Bt.TransactionRequest {
                Amount = context.Amount,
                PaymentMethodNonce = context.Nonce,
                MerchantAccountId = merchant,
                PurchaseOrderNumber = context.PurchaseOrder
            };
            if (config.AutomaticPayment) {
                request.Options = new Bt.TransactionOptionsRequest {
                    SubmitForSettlement = config.AutomaticPayment
                };
            }
            if (context.CustomFields != null) {
                request.CustomFields = context.CustomFields;
            }
            var gateway = GetGateway();
            Bt.Result<Bt.Transaction> payResult = gateway.Transaction.Sale(request);
            Bt.Transaction tran = null;
            result.Success = payResult.IsSuccess();
            if (payResult.Target != null) {
                // caso success == true
                tran = payResult.Target;
            } else if (payResult.Transaction != null) {
                // caso success == false
                tran = payResult.Transaction;
            }
            if (tran != null) {
                result.Amount = tran.Amount.Value;
                result.AuthorizationCode = tran.ProcessorAuthorizationCode;
                if (tran.BillingAddress != null) {
                    result.BillingAddress = string.Format("{0} - {1} {2} - {3} {4} - {5} {6} {7} - {8}",
                        tran.BillingAddress.Company,
                        tran.BillingAddress.FirstName,
                        tran.BillingAddress.LastName,
                        tran.BillingAddress.StreetAddress,
                        tran.BillingAddress.ExtendedAddress,
                        tran.BillingAddress.PostalCode,
                        tran.BillingAddress.Locality,
                        tran.BillingAddress.Region,
                        tran.BillingAddress.CountryName);
                }
                result.CurrencyIsoCode = tran.CurrencyIsoCode;
                if (tran.Customer != null) {
                    result.Customer = string.Format("{0} {1}, {2}, ({3})",
                        tran.Customer.FirstName,
                        tran.Customer.LastName,
                        tran.Customer.Company,
                        tran.Customer.Email);
                }
                result.MerchantAccountId = tran.MerchantAccountId;
                result.OrderId = tran.OrderId;
                result.PurchaseOrderNumber = tran.PurchaseOrderNumber;
                result.ResponseCode = tran.ProcessorResponseCode;
                result.ResponseText = tran.ProcessorResponseText;
                result.Status = tran.Status.ToString();
                result.TransactionId = tran.Id;
                result.Type = tran.Type.ToString();
            }
            result.Details = JsonConvert.SerializeObject(payResult);
            return result;
        }

        private Bt.BraintreeGateway GetGateway() {
            var config = _orchardServices.WorkContext.CurrentSite.As<BraintreeSiteSettingsPart>();
            var env = (config.ProductionEnvironment) ? Bt.Environment.PRODUCTION : Bt.Environment.SANDBOX;

            return new Bt.BraintreeGateway {
                Environment = env,
                MerchantId = config.MerchantId,
                PublicKey = config.PublicKey,
                PrivateKey = config.PrivateKey
            };
        }
    }
}