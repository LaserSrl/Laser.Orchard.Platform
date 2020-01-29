using Laser.Orchard.PaymentGateway.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Braintree.Services {
    public class PaymentContext {

        public PaymentContext() {
            CustomFields = new Dictionary<string, string>();
        }

        public PaymentContext(
            string nonce, decimal amount, Dictionary<string,string> customFields) 
            : this() {
            Nonce = nonce;
            Amount = amount;
            if (customFields != null) {
                CustomFields = customFields;
            }
        }

        public PaymentContext(
            string nonce, PaymentRecord paymentInfo) 
            : this(nonce, paymentInfo.Amount, null) {

            PaymentRecord = paymentInfo;
            PurchaseOrder = PaymentRecord.PaymentUniqueKey;
        }

        public PaymentRecord PaymentRecord { get; set; }
        public string Nonce { get; set; }
        public decimal Amount { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
        public string PurchaseOrder { get; set; }
    }
}