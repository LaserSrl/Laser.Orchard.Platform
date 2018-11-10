using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Braintree.Models
{
    public class TransactionResult
    {
        public string TransactionId { get; set; }
        public string CurrencyIsoCode { get; set; }
        public decimal Amount { get; set; }
        public bool Success { get; set; }
        public string MerchantAccountId { get; set; }
        public string Customer { get; set; }
        public string BillingAddress { get; set; }
        public string OrderId { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string AuthorizationCode { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseText { get; set; }
        public dynamic Details { get; set; }
    }
}