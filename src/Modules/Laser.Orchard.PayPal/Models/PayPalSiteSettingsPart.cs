using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PayPal.Models {
    public class PayPalSiteSettingsPart : ContentPart {
        public string SecretId {
            get { return this.Retrieve(x => x.SecretId); }
            set { this.Store(x => x.SecretId, value); }
        }
        public string ClientId {
            get { return this.Retrieve(x => x.ClientId); }
            set { this.Store(x => x.ClientId, value); }
        }
        public string CurrencyCode {
            get { return this.Retrieve(x => x.CurrencyCode); }
            set { this.Store(x => x.CurrencyCode, value); }
        }
        // Customize funding
        public bool Card {
            get { return this.Retrieve(x => x.Card); }
            set { this.Store(x => x.Card, value); }
        }
        public bool Credit {
            get { return this.Retrieve(x => x.Credit); }
            set { this.Store(x => x.Credit, value); }
        }
        public bool Bancontact {
            get { return this.Retrieve(x => x.Bancontact); }
            set { this.Store(x => x.Bancontact, value); }
        }
        public bool Blik {
            get { return this.Retrieve(x => x.Blik); }
            set { this.Store(x => x.Blik, value); }
        }
        public bool Eps {
            get { return this.Retrieve(x => x.Eps); }
            set { this.Store(x => x.Eps, value); }
        }
        public bool Giropay {
            get { return this.Retrieve(x => x.Giropay); }
            set { this.Store(x => x.Giropay, value); }
        }
        public bool Ideal {
            get { return this.Retrieve(x => x.Ideal); }
            set { this.Store(x => x.Ideal, value); }
        }
        public bool Mybank {
            get { return this.Retrieve(x => x.Mybank); }
            set { this.Store(x => x.Mybank, value); }
        }
        public bool Przelewy {
            get { return this.Retrieve(x => x.Przelewy); }
            set { this.Store(x => x.Przelewy, value); }
        }
        public bool Sepa {
            get { return this.Retrieve(x => x.Sepa); }
            set { this.Store(x => x.Sepa, value); }
        }
        public bool Sofort {
            get { return this.Retrieve(x => x.Sofort); }
            set { this.Store(x => x.Sofort, value); }
        }
        public bool Venmo {
            get { return this.Retrieve(x => x.Venmo); }
            set { this.Store(x => x.Venmo, value); }
        }        
    }
}