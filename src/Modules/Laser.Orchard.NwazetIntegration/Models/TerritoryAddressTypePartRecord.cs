using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class TerritoryAddressTypePartRecord : ContentPartRecord {
        public virtual bool Shipping { get; set; }
        public virtual bool Billing { get; set; }
        public virtual TerritoryInternalRecord TerritoryInternalRecord { get; set; }
    }
}