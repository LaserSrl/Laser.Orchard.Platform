using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Braintree.Models
{
    public class BraintreeSiteSettingsPart : ContentPart
    {
        public bool ProductionEnvironment
        {
            get { return this.Retrieve(x => x.ProductionEnvironment); }
            set { this.Store(x => x.ProductionEnvironment, value); }
        }
        public string MerchantId
        {
            get { return this.Retrieve(x => x.MerchantId); }
            set { this.Store(x => x.MerchantId, value); }
        }
        public string CurrencyCode {
            get { return this.Retrieve(x => x.CurrencyCode); }
            set { this.Store(x => x.CurrencyCode, value); }
        }
        public string PublicKey
        {
            get { return this.Retrieve(x => x.PublicKey); }
            set { this.Store(x => x.PublicKey, value); }
        }
        public string PrivateKey
        {
            get { return this.Retrieve(x => x.PrivateKey); }
            set { this.Store(x => x.PrivateKey, value); }
        }

        public string MerchantAccountId {
            get { return this.Retrieve(x => x.MerchantAccountId); }
            set { this.Store(x => x.MerchantAccountId, value); }
        }

        public bool AutomaticPayment {
            get { return this.Retrieve(x => x.AutomaticPayment); }
            set { this.Store(x => x.AutomaticPayment, value); }
        }
    }
}