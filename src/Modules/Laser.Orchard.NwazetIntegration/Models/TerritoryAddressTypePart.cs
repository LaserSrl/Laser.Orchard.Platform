using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class TerritoryAddressTypePart :
        ContentPart<TerritoryAddressTypePartRecord> {
        // this will "enable" use of the territory for address configurations
        // for Shipping or Billing (or both)
        public bool Shipping {
            get { return Retrieve(r => r.Shipping); }
            set { Store(r => r.Shipping, value); }
        }
        public bool Billing {
            get { return Retrieve(r => r.Billing); }
            set { Store(r => r.Billing, value); }
        }
    }
}