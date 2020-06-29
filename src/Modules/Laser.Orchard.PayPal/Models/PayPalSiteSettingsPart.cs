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
    }
}