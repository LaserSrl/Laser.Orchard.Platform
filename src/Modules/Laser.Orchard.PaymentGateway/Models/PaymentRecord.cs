using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.Models {
    public class PaymentRecord {
        public virtual int Id { get; set; }
        public virtual string Reason { get; set; }
        public virtual string PosName { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual DateTime UpdateDate { get; set; }
        public virtual string PosUrl { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual string Currency { get; set; }
        public virtual bool Success { get; set; }
        public virtual string Error { get; set; }
        public virtual string TransactionId { get; set; }
        public virtual string Info { get; set; }
        public virtual int ContentItemId { get; set; }
        public virtual int UserId { get; set; }
        //update 2016/09/22: add new properties to store custom redirect information
        public virtual string CustomRedirectUrl { get; set; }
        public virtual string CustomRedirectSchema { get; set; }
        public virtual bool PaymentTransactionComplete { get; set; }
        public virtual string Guid { get; set; }

        public virtual string APIFilters { get; set; } //comma separated list of the selected API filters

        public static string[] ValidAPIFilters = { "REASON", "POSNAME", "AMOUNT", "CURRENCY", "ERROR", "INFO", "USERID"
                                                  };
        public static bool IsValidAPIFilter(string fil) {
            return ValidAPIFilters.Contains(fil.ToUpperInvariant());
        }
        public string GetProperty(string name) {
            if (!IsValidAPIFilter(name))
                return null;
            name = name.ToUpperInvariant();
            switch (name) {
                case "REASON":
                    return this.Reason;
                case "POSNAME":
                    return this.PosName;
                case "AMOUNT":
                    return this.Amount.ToString("0,##", CultureInfo.InvariantCulture);
                case "CURRENCY":
                    return this.Currency;
                case "ERROR":
                    return this.Error;
                case "INFO":
                    return this.Info;
                case "USERID":
                    return this.UserId.ToString();
                default:
                    return null;
            }
        }

        // Property added to store unique payent key While not creating a dependency
        // on OrderPart from Nwazet.Commerce
        public string PaymentUniqueKey { get; set; }
    }
}